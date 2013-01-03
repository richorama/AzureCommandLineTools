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

namespace CopyBlob
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                string usage = @"Usage: CopyBlob container\blob container\blob";
                Trace.TraceInformation(usage);
                Console.WriteLine(usage);
                return (Settings.SUCCESS);
            }

            string blobPath1 = args[0];
            string blobPath2 = args[1];

            string[] fields1 = blobPath1.Split('/');
            string containerName1 = fields1[0];
            string blobName1 = fields1[1];

            string[] fields2 = blobPath2.Split('/');
            string containerName2 = fields2[0];
            string blobName2 = fields2[1];

            BlobHelper blobHelper = new BlobHelper();
            blobHelper.CopyBlob(containerName1, blobName1, containerName2, blobName2);

            return (Settings.SUCCESS);
        }
    }
}
