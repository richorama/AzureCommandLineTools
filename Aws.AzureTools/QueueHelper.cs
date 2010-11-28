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
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Aws.AzureTools
{
    public class QueueHelper
    {
        CloudQueueClient cloudQueueClient;

        public QueueHelper()
        {
            string connectionString = Settings.AzureConnectionString();

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();

            cloudQueueClient.Timeout = Settings.Timeout();
            cloudQueueClient.RetryPolicy = RetryPolicies.Retry(Settings.RetryCount(), TimeSpan.FromSeconds(3));
        }

        public IEnumerable<CloudQueue> ListQueues()
        {
            return cloudQueueClient.ListQueues();
        }
    }
}
