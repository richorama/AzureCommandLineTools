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
using Microsoft.WindowsAzure.StorageClient;

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

            cloudBlobClient.Timeout = Settings.Timeout();
            cloudBlobClient.RetryPolicy = RetryPolicies.Retry(Settings.RetryCount(), TimeSpan.FromSeconds(3));
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
            CloudBlob blob = container.GetBlobReference(blobName);

            blob.DownloadToFile(filename);
        }

        public void CopyBlob(string containerName1, string blobName1, string containerName2, string blobName2)
        {
            CloudBlobContainer container1 = cloudBlobClient.GetContainerReference(containerName1);
            CloudBlobContainer container2 = cloudBlobClient.GetContainerReference(containerName2);

            CloudBlob blob1 = container1.GetBlobReference(blobName1);
            CloudBlob blob2 = container1.GetBlobReference(blobName2);

            blob2.CopyFromBlob(blob1);
        }

        public void DeleteBlob(string containerName, string blobName)
        {
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);
            CloudBlob blob = container.GetBlobReference(blobName);

            blob.Delete();
        }

        public void PutBlob(string filename, string containerName, string blobName)
        {
           
            CloudBlobContainer container = cloudBlobClient.GetContainerReference(containerName);

            container.CreateIfNotExist();

            Trace.TraceInformation("UploadingBlob..");

            container.GetBlobReference(blobName).UploadFile(filename);
            Trace.TraceInformation("Done");
        }

        public void PutBlob(string filename, string blobName)
        {

            CloudBlob b = cloudBlobClient.GetBlobReference(blobName);

            b.UploadFile(filename);
            Trace.TraceInformation("Done");
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
