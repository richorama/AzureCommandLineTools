AzureCommandLineTools
=====================

Simple command line tools for interacting with Windows Azure from a command prompt or batch file in Windows.

The tools compile to a subdirectory called "dist" - edit and run the setup.bat file in there to get started.

Setup.bat
---------

Before running these commands you need to set an environment variable with your azure storage configuration details. Most people will need something like this:

	SET AZURE_CONNECTION_STRING=DefaultEndpointsProtocol=https;AccountName=YOURACCOUNTNAME;AccountKey=YOURACCOUNTKEY

or for more advanced usage, see http://blogs.msdn.com/b/partlycloudy/archive/2009/12/08/configuring-the-storage-client-with-connection-strings.aspx, for example:

	SET AZURE_CONNECTION_STRING=TableEndpoint=https://YOURACCOUNTNAME.TABLE.COM/;QueueEndpoint=https://YOURACCOUNTNAME.QUEUE.COM/;BlobEndpoint=https://YOURACCOUNTNAME.BLOB.COM/;AccountName=YOURACCOUNTNAME;AccountKey=YOURACCOUNTKEY

You can optionally set environment variables for TIMEOUT (in seconds) and RETRY_COUNT - particulalrly useful if you are on a slow connection.

I've also included a slightly modified version of TraceConsole, the program from the AppFabric samples that allows you to
see debug output locally. This is used in AzureRunMe.

In order to use it, you can set environment vairables like this:

	SET CLOUD_TRACE_SERVICE_PATH=YOURSERVICEPATH
	SET CLOUD_TRACE_SERVICE_NAMESPACE=YOURNAMESPACE
	SET CLOUD_TRACE_ISSUER_NAME=YOURISSUERNAME
	SET CLOUD_TRACE_ISSUER_SECRET=YOURISSUERSECRET

These settings are used by trace.exe and traceconsole.exe.

It's convenient to put these in a batch file like setup.bat - I create a copy of setup.bat for each of my projects.


Examples
--------

	ListContainers

	ListBlobs

	ListBlobs mycontainer

	PutBlob myfilename mycontainer

	PutBlob myfilename mycontainer/myblob

	GetBlob mycontainer/myblob

	DeleteBlob mycontainer/myblob

	CopyBlob mycontainer/myblob1 mycontainer/myblob2

	ListQueues

	ListTables

	CreateTable table

	DeleteTable table

	TouchBlob mycontainer/myblob

	TraceConsole

	Trace "Hello World"


Rob Blackwell

May 2011