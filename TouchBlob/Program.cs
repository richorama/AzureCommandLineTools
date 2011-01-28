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
using Aws.AzureTools;
using System.Diagnostics;

namespace TouchBlob
{
    class Program
    {
        static int Main(string[] args)
        {

            if (args.Length != 1)
            {
                string usage = "Usage: TouchBlob container/blob";
                Trace.TraceInformation(usage);
                Console.WriteLine(usage);
                return (Settings.SUCCESS);
            }

            string blobPath = args[0];

            string[] fields = blobPath.Split('/');
            string containerName = fields[0];
            string blobName = fields[1];

            BlobHelper blobHelper = new BlobHelper();
            blobHelper.TouchBlob(containerName, blobName);

            return (Settings.SUCCESS);
        }
    }
}
