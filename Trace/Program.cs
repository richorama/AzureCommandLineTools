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
using System.Configuration;
using Microsoft.ServiceBus.Samples;

namespace Trace
{
    class Program
    {
        static void Main(string[] args)
        {
            string servicePath = Environment.GetEnvironmentVariable("CLOUD_TRACE_SERVICE_PATH");
            if (servicePath == null)
                servicePath = ConfigurationManager.AppSettings["CloudTraceServicePath"];

            string serviceNamespace = Environment.GetEnvironmentVariable("CLOUD_TRACE_SERVICE_NAMESPACE");
            if (serviceNamespace == null)
                serviceNamespace = ConfigurationManager.AppSettings["CloudTraceServiceNamespace"];

            string issuerName = Environment.GetEnvironmentVariable("CLOUD_TRACE_ISSUER_NAME");
            if (issuerName == null)
                issuerName = ConfigurationManager.AppSettings["CloudTraceIssuerName"];

            string issuerSecret = Environment.GetEnvironmentVariable("CLOUD_TRACE_ISSUER_SECRET");
            if (issuerSecret == null)
                issuerSecret = ConfigurationManager.AppSettings["CloudTraceIssuerSecret"];

            CloudTraceListener cloudTraceListener = new CloudTraceListener(servicePath, serviceNamespace, issuerName, issuerSecret);

            string message = args[0];

            cloudTraceListener.WriteLine(string.Format("{0}: {1:u} {2}", Environment.MachineName, DateTime.Now, message), "Information");
            
        }
    }
}
