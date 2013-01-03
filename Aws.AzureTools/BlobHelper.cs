#region Copyright (c) 2010 Active Web Solutions Ltd
//
// (C) Copyright 2010 Active Web Solutions Ltd
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
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System.Text;

namespace Aws.AzureTools
{
    public class BlobHelper
    {
        CloudBlobClient cloudBlobClient;

        public BlobHelper()
        {
            string connectionString = Settings.AzureConnectionString();

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            cloudBlobClient.ServerTimeout = Settings.Timeout();
            cloudBlobClient.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), Settings.RetryCount());
        }

        public IEnumerable<CloudBlobContainer> ListContainers()
        {
            return cloudBlobClient.ListContainers();
        }

        public IEnumerable<IListBlobItem> ListBlobs(string containerName)
        {
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

            return cloudBlobContainer.ListBlobs();
        }

        public void GetBlob(string containerName, string blobName, string filename)
        {
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            ICloudBlob blob = container.GetBlobReferenceFromServer(blobName);
            using (var fs = File.Create(filename))
            {
                blob.DownloadToStream(fs);
            }
        }

        public void TouchBlob(string containerName, string blobName)
        {
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.UploadFromStream(new MemoryStream(Encoding.UTF8.GetBytes("")));
            //blob.FetchAttributes();
            blob.SetProperties();
        }

        public void CopyBlob(string containerName1, string blobName1, string containerName2, string blobName2)
        {
            CloudBlobContainer container1 = cloudBlobClient.GetContainerReference(containerName1);
            CloudBlobContainer container2 = cloudBlobClient.GetContainerReference(containerName2);

            CloudBlockBlob blob1 = container1.GetBlockBlobReference(blobName1);
            CloudBlockBlob blob2 = container2.GetBlockBlobReference(blobName2);

            blob2.StartCopyFromBlob(blob1.Uri);
            /// TODO, add wiat logic to check the Copy State and then return
        }

        public void DeleteBlob(string containerName, string blobName)
        {
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            ICloudBlob blob = container.GetBlobReferenceFromServer(blobName);

            blob.Delete();
        }

        public void PutBlob(string filename, string containerName, string blobName)
        {
            if (!File.Exists(filename))
            {
                throw new ArgumentException("The file to upload must be an existing file", "filename");
            }

            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            Trace.TraceInformation("UploadingBlob..");

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            blob.Properties.ContentType = MimeTypeHelper.GetMimeType(filename);
            using(var fs = File.OpenRead(filename))
            {
                blob.UploadFromStream(fs);
            }
            Trace.TraceInformation("Done");
        }

        public void PutBlob(string filename, string blobName)
        {
            var containerName = blobName.Substring(0, blobName.IndexOf("/"));
            var realBlobName = blobName.Substring(blobName.IndexOf("/") + 1);

            this.PutBlob(filename, containerName, realBlobName);
        }


        public void PutLargeBlob(string filename, string containerName, string blobName)
        {
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();

            cloudBlobClient.SingleBlobUploadThresholdInBytes = Settings.WriteBlockSizeInBytes();

            Trace.TraceInformation("UploadingBlob..");

            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);
            blob.Properties.ContentType = MimeTypeHelper.GetMimeType(filename);

            
            blob.ParallelUpload(filename, null);

            Trace.TraceInformation("Done");
        }

        public void PutLargeBlob(string filename, string blobName)
        {
            var containerName = blobName.Substring(0, blobName.IndexOf("/"));
            var realBlobName = blobName.Substring(blobName.IndexOf("/") + 1);
            this.PutLargeBlob(filename, containerName, realBlobName);
        }

        
        public void DeleteContainer(string containerName)
        {
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);

            container.Delete();
        }

        public static bool IsBlobReference(string s)
        {
            return (s.LastIndexOf('/') > 0);

        }

        public static string Display(Uri u)
        {
            return u.PathAndQuery.Remove(0, 1);
        }
    }
}
