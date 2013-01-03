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
using System.Diagnostics;
using Aws.AzureTools;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ListBlobs
{
    class Program
    {
        static int Main(string[] args)
        {
            string containerName = Settings.Container();

            if (args.Length == 1)
            {
                containerName = args[0];

                BlobHelper blobHelper = new BlobHelper();
                foreach (IListBlobItem blob in blobHelper.ListBlobs(containerName))
                {
                    ICloudBlob cloudBlob = blob as ICloudBlob;

                    if (cloudBlob != null)
                    {
                        Console.WriteLine(String.Format("{0} {1}", BlobHelper.Display(blob.Uri), cloudBlob.Properties.LastModified));
                    }
                    else
                    {
                        Console.WriteLine(BlobHelper.Display(blob.Uri));
                    }
                }
            }

            else
            {
                BlobHelper blobHelper = new BlobHelper();
                foreach (CloudBlobContainer container in blobHelper.ListContainers())
                {
                    foreach (IListBlobItem blob in container.ListBlobs())
                    {
                        ICloudBlob cloudBlob = blob as ICloudBlob;

                        if (cloudBlob != null)
                        {
                            Console.WriteLine(String.Format("{0} {1}", BlobHelper.Display(blob.Uri), cloudBlob.Properties.LastModified));
                        }
                        else
                        {
                            Console.WriteLine(BlobHelper.Display(blob.Uri));
                        }
                    }
                }
            }

            return (Settings.SUCCESS);
        }
    }
}
