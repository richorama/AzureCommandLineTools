#region Copyright (c) 2011 Active Web Solutions Ltd
//
// (C) Copyright 2011 Active Web Solutions Ltd
//      All rights reserved.
//
// This software is provided "as is" without warranty of any kind,
// express or implied, including but not limited to warranties as to
// quality and fitness for a particular purpose. Active Web Solutions Ltd
// does not support the Software, nor does it warrant that the Software
// will meet your requirements or that the operation of the Software will
// be uninterrupted or error free or that any defects will be
// corrected. Nothing in this statement is intended to limit or exclude
// any liability for personal injury or death caused by the negligence of
// Active Web Solutions Ltd, its employees, contractors or agents.
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;
using System.Security.Cryptography;
using System.Threading;

namespace Aws.AzureTools
{
    // http://blogs.msdn.com/b/windowsazurestorage/archive/2011/02/23/windows-azure-storage-client-library-parallel-single-blob-upload-race-condition-can-throw-an-unhandled-exception.aspx

    public static class BlobExtensions
    {

        public static void ParallelUpload(this CloudBlockBlob blobRef, string filename, BlobRequestOptions options)
        {
            if (null == options)
            {
                options = new BlobRequestOptions()
                {
                    Timeout = blobRef.ServiceClient.Timeout,
                    RetryPolicy = RetryPolicies.RetryExponential(RetryPolicies.DefaultClientRetryCount, RetryPolicies.DefaultClientBackoff)
                };
            }

            // get upload history if any 
            UploadInfo uploadInfo = UploadInfo.LoadByUploadFilename(filename);

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                blobRef.ParallelUpload(fs, uploadInfo, options);
            }

            // upload completed no history needed - delete it
            if (File.Exists(uploadInfo.LogFilename))
                File.Delete(uploadInfo.LogFilename);

            Console.WriteLine("\nUpload completed.");
        }

        private static void ParallelUpload(this CloudBlockBlob blobRef, Stream sourceStream, UploadInfo uploadInfo, BlobRequestOptions options)
        {
            List<IAsyncResult> asyncResults = new List<IAsyncResult>();
            List<BlockInfo> blockInfoList = uploadInfo.BlockInfoList;

            // set stream position based on read uploadInfo
            int blockSize = (int)blobRef.ServiceClient.WriteBlockSizeInBytes;
            bool moreToUpload = (sourceStream.Length - sourceStream.Position > 0);
            int currentBlockPossition = blockInfoList.Count;

            long totalBytes = sourceStream.Length;
            long uploadedBytes = 0;

            using (MD5 fullBlobMD5 = MD5.Create())
            {
                // re-create file hash if starting again
                if (currentBlockPossition > 0)
                {
                    for (int i = 0; i < currentBlockPossition; i++)
                    {
                        int totalCopied = 0, numRead = 0;
                        int blockBufferSize = (int)Math.Min(blockSize, sourceStream.Length - sourceStream.Position);
                        byte[] buffer = new byte[blockBufferSize];

                        do
                        {
                            numRead = sourceStream.Read(buffer, totalCopied, blockBufferSize - totalCopied);
                            totalCopied += numRead;
                        }
                        while (numRead != 0 && totalCopied < blockBufferSize);

                        fullBlobMD5.TransformBlock(buffer, 0, totalCopied, null, 0);
                    }

                    uploadedBytes = sourceStream.Position;
                }


                do
                {
                    int currentPendingTasks = asyncResults.Count;

                    for (int i = currentPendingTasks; i < blobRef.ServiceClient.ParallelOperationThreadCount && moreToUpload; i++)
                    {
                        // Step 1: Create block streams in a serial order as stream can only be read sequentially
                        string blockId = null;

                        // Dispense Block Stream
                        int totalCopied = 0, numRead = 0;
                        MemoryStream blockAsStream = null;
                        uploadInfo.BlockIdSequenceNumber++;

                        int blockBufferSize = (int)Math.Min(blockSize, sourceStream.Length - sourceStream.Position);
                        byte[] buffer = new byte[blockBufferSize];
                        blockAsStream = new MemoryStream(buffer);

                        do
                        {
                            numRead = sourceStream.Read(buffer, totalCopied, blockBufferSize - totalCopied);
                            totalCopied += numRead;
                        }
                        while (numRead != 0 && totalCopied < blockBufferSize);


                        // Update Running MD5 Hashes
                        fullBlobMD5.TransformBlock(buffer, 0, totalCopied, null, 0);
                        blockId = GenerateBase64BlockID(uploadInfo.BlockIdSequenceNumber);

                        // Step 2: Fire off consumer tasks that may finish on other threads
                        BlockInfo blockInfo = new BlockInfo { OrderPosition = currentBlockPossition++, BlockId = blockId };
                        blockInfoList.Add(blockInfo);

                        IAsyncResult asyncresult = blobRef.BeginPutBlock(blockId, blockAsStream, null, options, null,
                            new UploadState { BlockAsStream = blockAsStream, BlockInfo = blockInfo });
                        asyncResults.Add(asyncresult);

                        if (sourceStream.Length == sourceStream.Position)
                        {
                            // No more upload tasks
                            moreToUpload = false;
                        }
                    }

                    // Step 3: Wait for 1 or more put blocks to finish and finish operations
                    if (asyncResults.Count > 0)
                    {
                        int waitTimeout = options.Timeout.HasValue ? (int)Math.Ceiling(options.Timeout.Value.TotalMilliseconds) : Timeout.Infinite;
                        int waitResult = WaitHandle.WaitAny(asyncResults.Select(result => result.AsyncWaitHandle).ToArray(), waitTimeout);

                        if (waitResult == WaitHandle.WaitTimeout)
                        {
                            throw new TimeoutException(String.Format("ParallelUpload Failed with timeout = {0}", options.Timeout.Value));
                        }

                        // Optimize away any other completed operations
                        for (int index = 0; index < asyncResults.Count; index++)
                        {
                            IAsyncResult result = asyncResults[index];
                            if (result.IsCompleted)
                            {
                                // Dispose of memory stream
                                var uploadState = result.AsyncState as UploadState;
                                uploadedBytes += uploadState.BlockAsStream.Length;
                                (uploadState.BlockAsStream as IDisposable).Dispose();

                                asyncResults.RemoveAt(index);
                                blobRef.EndPutBlock(result);
                                index--;

                                // log uploaded block
                                UploadInfo.LogUploadProgress(uploadInfo.LogFilename, uploadState.BlockInfo);

                                // output progress
                                Console.Write("\b\b\b\b");
                                Console.Write(" {0}%", (uploadedBytes * 100) / (totalBytes));
                            }
                        }
                    }
                }
                while (moreToUpload || asyncResults.Count != 0);

                // Step 4: Calculate MD5 and do a PutBlockList to commit the blob
                fullBlobMD5.TransformFinalBlock(new byte[0], 0, 0);
                byte[] blobHashBytes = fullBlobMD5.Hash;
                string blobHash = Convert.ToBase64String(blobHashBytes);
                blobRef.Properties.ContentMD5 = blobHash;

                List<string> blockList = blockInfoList.OrderBy(b => b.OrderPosition).Select(b => b.BlockId).ToList();
                blobRef.PutBlockList(blockList, options);
            }
        }

        /// <summary>
        /// Generates a unique Base64 encoded blockID
        /// </summary>
        /// <param name="seqNo">The blocks sequence number in the given upload operation.</param>
        /// <returns></returns>
        private static string GenerateBase64BlockID(long seqNo)
        {
            // 9 bytes needed since base64 encoding requires 6 bits per character (6*12 = 8*9)
            byte[] tempArray = new byte[9];

            for (int m = 0; m < 9; m++)
            {
                tempArray[8 - m] = (byte)((seqNo >> (8 * m)) & 0xFF);
            }

            Convert.ToBase64String(tempArray);

            return Convert.ToBase64String(tempArray);
        }


    }


}

