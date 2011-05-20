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

namespace Aws.AzureTools
{
    public class Settings
    {
        public const int SUCCESS = 0;
        public const int FAIL = 1;

        public static string AzureConnectionString()
        {
            string s = Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING");
            if (s == null)
                throw new Exception("AZURE_CONNECTION_STRING is not set");

            Trace.TraceInformation("AZURE_CONNECTION_STRING:{0}", s);
            return s;
        }

        public static string Container()
        {
            string s = Environment.GetEnvironmentVariable("CONTAINER");
            Trace.TraceInformation("CONTAINER:{0}", s);
            return s;
        }

        public static TimeSpan Timeout()
        {
            string s = Environment.GetEnvironmentVariable("TIMEOUT");
            TimeSpan t;

            if (s == null)
                t = TimeSpan.FromMinutes(30);
            else
                t = TimeSpan.FromSeconds(int.Parse(s));

            Trace.TraceInformation("TIMEOUT:{0}", t.Seconds);
            return t;
        }

        public static int RetryCount()
        {
            string s = Environment.GetEnvironmentVariable("RETRY_COUNT");
            int i;

            if (s == null)
                i = 3;
            else
                i = int.Parse(s);

            Trace.TraceInformation("RETRY_COUNT:{0}", i);
            return i;
        }

        public static int WriteBlockSizeInBytes()
        {
            int blockSizeInMB = 4; // values 1 - 4
            return (1024 * 1024 * blockSizeInMB);
        }


    }
}
