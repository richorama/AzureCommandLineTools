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
using System.IO;

namespace PutBlob
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
                string usage = "Usage: PutBlob filename [containername[/blobname]]";
                Trace.TraceInformation(usage);
                Console.WriteLine(usage);
                return(Settings.SUCCESS);
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
                
            if (BlobHelper.IsBlobReference(destination))
            {
                blobHelper.PutBlob(filename, destination);
            }
            else
            {
                // Use the filename as the blob name
                blobName = new FileInfo(filename).Name;
                Trace.TraceInformation("blobName:{0}", blobName);
                    
                blobHelper.PutBlob(filename, destination, blobName);
            }
                 
            return (Settings.SUCCESS);
        }
    }
}
