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
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;

namespace TraceProcess
{
    class Program
    {

        private CloudTraceListener cloudTraceListener;

        private Program(CloudTraceListener cloudTraceListener)
        {
            this.cloudTraceListener = cloudTraceListener;
        }

        private void Write(string message)
        {
            Console.WriteLine(message);

            ThreadPool.QueueUserWorkItem(new WaitCallback(Log), message);
        }

        private void Log(object message)
        {
            cloudTraceListener.WriteLine(string.Format("{0}: {1:u} {2}", Environment.MachineName, DateTime.Now, message), "Information");
        }

        public void CopyStream(TextReader textReader, StreamWriter streamWriter)
        {
            char[] buffer = new char[256];

            int i;
            while ((i = textReader.Read(buffer, 0, buffer.Length)) > 0)
            {
                streamWriter.Write(buffer, 0, i);
                streamWriter.Flush();

            }
        }



        private int RunCommand(string command, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(command, arguments)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            Process process = new Process()
            {
                StartInfo = startInfo
            };

            process.ErrorDataReceived += (sender, e) => { Write(e.Data); };
            process.OutputDataReceived += (sender, e) => { Write(e.Data); };
           
            process.Start();
            process.BeginOutputReadLine(); 
            process.BeginErrorReadLine();

            CopyStream(Console.In, process.StandardInput);

            return process.ExitCode;
        }

        static void PrintUsage()
        {
            Console.WriteLine("TraceProcess <process> [<args>]");
        }

        static int Main(string[] args)
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

            Program program = new Program(cloudTraceListener);

            string command = "";
            string arguments = "";

            if (args.Length > 0)
            {
                command = args[0];
                if (args.Length > 1)
                    arguments = args[1];

                return program.RunCommand(command, arguments);
            
            }
            else
            {
                PrintUsage();
                return 0;
            }      
        }
    }
}
