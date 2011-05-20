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

namespace Aws.AzureTools
{
    public class UploadInfo
    {
        public string LogFilename { get; set; }
        public long BlockIdSequenceNumber { get; set; }
        public List<BlockInfo> BlockInfoList { get; set; }

        public UploadInfo(string uploadFilename)
        {
            Random rand = new Random();
            long blockIdSequenceNumber = (long)rand.Next() << 32;
            this.BlockIdSequenceNumber += rand.Next();

            this.LogFilename = GetLogFilename(Path.GetFileName(uploadFilename));

            this.BlockInfoList = new List<BlockInfo>();
        }


        public static void LogUploadProgress(string logFilename, BlockInfo blockInfo)
        {
            using (FileStream fs = new FileStream(logFilename, FileMode.Append, FileAccess.Write))
            {
                byte[] data = Encoding.ASCII.GetBytes(String.Format("{0}|{1}{2}", blockInfo.OrderPosition, blockInfo.BlockId, Environment.NewLine));
                fs.Write(data, 0, data.Length);
            }
        }

        public static UploadInfo LoadByUploadFilename(string uploadFilename)
        {
            UploadInfo uploadInfo = new UploadInfo(uploadFilename);

            if (File.Exists(uploadInfo.LogFilename))
            {
                // read previous uploaded blocks
                SortedDictionary<int, BlockInfo> sortedInfo = new SortedDictionary<int, BlockInfo>();
                string line;
                using (StreamReader sr = new StreamReader(uploadInfo.LogFilename))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] data = line.Split('|');
                        BlockInfo blockInfo = new BlockInfo { OrderPosition = Int32.Parse(data[0]), BlockId = data[1] };
                        sortedInfo.Add(blockInfo.OrderPosition, blockInfo);
                    }
                }

                // fill continous BlockInfoList and overwrite the log file with current list
                if (File.Exists(uploadInfo.LogFilename))
                    File.Delete(uploadInfo.LogFilename);

                for (int i = 0; i < sortedInfo.Count; i++)
                {
                    if (sortedInfo.ContainsKey(i))
                    {
                        uploadInfo.BlockInfoList.Add(sortedInfo[i]);
                        LogUploadProgress(uploadInfo.LogFilename, sortedInfo[i]);
                    }
                    else
                    {
                        break;
                    }
                }

            }

            return uploadInfo;
        }

        private string GetLogFilename(string uploadFilename)
        {
            return Path.GetFileName(uploadFilename) + ".putblob";
        }

    }
}
