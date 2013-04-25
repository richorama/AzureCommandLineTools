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
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace Aws.AzureTools
{
    public class TableHelper
    {
        CloudTableClient cloudTableClient;

        public TableHelper()
        {
            string connectionString = Settings.AzureConnectionString();

            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            cloudTableClient = cloudStorageAccount.CreateCloudTableClient();

            cloudTableClient.ServerTimeout = Settings.Timeout();
            cloudTableClient.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), Settings.RetryCount());
        }

        public IEnumerable<String> ListTables()
        {
            return cloudTableClient.ListTables().Select(ct => ct.Name);
        }

        public IEnumerable<IDictionary<string, string>> QueryTable(string tableName, string oDataQuery)
        {
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException("tableName");

            Console.WriteLine(tableName);
            var table = cloudTableClient.GetTableReference(tableName);

            var query = new TableQuery();
            
            if (!string.IsNullOrWhiteSpace(oDataQuery))
            {
                query.FilterString = oDataQuery;
            }
            foreach (var entity in table.ExecuteQuery(query))
            {
                var dictionary = new Dictionary<string, string>();
                dictionary.Add("PartitionKey", entity.PartitionKey);
                dictionary.Add("RowKey", entity.RowKey);
                dictionary.Add("Timestamp", entity.Timestamp.ToString());
                //dictionary.Add("Etag", entity.ETag);
                foreach (var property in entity.Properties)
                {
                    dictionary.Add(property.Key, property.Value.StringValue);
                }
                yield return dictionary;
            }
        }

        public void DeleteTable(string tableName)
        {
            this.cloudTableClient.GetTableReference(tableName).DeleteIfExists();
        }

        public void CreateTable(string tableName)
        {

            this.cloudTableClient.GetTableReference(tableName).CreateIfNotExists();
        }



    }
}
