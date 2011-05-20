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
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using Aws.AzureTools;
using System.Diagnostics;

namespace PutLargeBlob
{
    class Program
    {
        static int Main(string[] args)
        {

            string filename = "";
            string destination = Settings.Container();

            if (args.Length == 1)
            {
                filename = args[0];
                Trace.TraceInformation("filename:{0}", filename);
            }
            else if (args.Length == 2)
            {
                filename = args[0];
                Trace.TraceInformation("filename:{0}", filename);
                destination = args[1];
                Trace.TraceInformation("destination:{0}", destination);
            }
            else
            {
                string usage = "Usage: PutLargeBlob filename [containername[/blobname]]";
                Trace.TraceInformation(usage);
                Console.WriteLine(usage);
                return (Settings.SUCCESS);
            }

            if (destination == null)
            {
                string s = "No CONTAINER or destination specified";
                Console.WriteLine(s);
                Trace.TraceError(s);
                return (Settings.FAIL);
            }


            BlobHelper blobHelper = new BlobHelper();
            string blobName;

            try
            {
                if (BlobHelper.IsBlobReference(destination))
                {
                    blobHelper.PutLargeBlob(filename, destination);
                }
                else
                {
                    // Use the filename as the blob name
                    blobName = new FileInfo(filename).Name;
                    Trace.TraceInformation("blobName:{0}", blobName);

                    blobHelper.PutLargeBlob(filename, destination, blobName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading blob. Please run upload again with the same parameters.");
                Console.Error.WriteLine(ex.ToString());

                Trace.TraceError(ex.ToString());
                return (Settings.FAIL);
            }

            return (Settings.SUCCESS);

        }


        


    }
}
