using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BatchComposer
{
    class Program
    {
        // Batch account credentials
        private const string BatchAccountName = "emailworkbatch";
        // Add it locally
        private const string BatchAccountKey = "";
        private const string BatchAccountUrl = "https://emailworkbatch.eastasia.batch.azure.com";

        // Batch resource settings
        private const string PoolId = "DotNetQuickstartPool";
        private const string JobId = "DotNetQuickstartJob";
        private const int PoolNodeCount = 2;
        private const string PoolVMSize = "STANDARD_A1_v2";

        static void Main(string[] args)
        {
            if (String.IsNullOrEmpty(BatchAccountName) ||
                String.IsNullOrEmpty(BatchAccountKey) ||
                String.IsNullOrEmpty(BatchAccountUrl))
            {
                throw new InvalidOperationException("One or more account credential strings have not been populated. Please ensure that your Batch and Storage account credentials have been specified.");
            }

            try
            {
                Console.WriteLine("Sample start: {0}", DateTime.Now);
                Console.WriteLine();
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // Get a Batch client using account creds
                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(BatchAccountUrl, BatchAccountName, BatchAccountKey);

                using (BatchClient batchClient = BatchClient.Open(cred))
                {
                    Console.WriteLine("Creating pool [{0}]...", PoolId);

                    // Create a Windows Server image, VM configuration, Batch pool
                    ImageReference imageReference = CreateImageReference();

                    VirtualMachineConfiguration vmConfiguration = CreateVirtualMachineConfiguration(imageReference);

                    CreateBatchPool(batchClient, vmConfiguration);

                    // Create a Batch job
                    Console.WriteLine("Creating job [{0}]...", JobId);

                    try
                    {
                        CloudJob job = batchClient.JobOperations.CreateJob();
                        job.Id = JobId;
                        job.PoolInformation = new PoolInformation { PoolId = PoolId };

                        job.Commit();
                    }
                    catch (BatchException be)
                    {
                        // Accept the specific error code JobExists as that is expected if the job already exists
                        if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.JobExists)
                        {
                            Console.WriteLine("The job {0} already existed when we tried to create it", JobId);
                        }
                        else
                        {
                            throw; // Any other exception is unexpected
                        }
                    }

                    //Create a collection of all records from the database and send the same
                    List<string> records = new List<string>();

                    Console.WriteLine("Adding {0} tasks to job [{1}]...", records.Count, JobId);

                    List<CloudTask> tasks = new List<CloudTask>();

                    // Create each of the tasks to process one of the records. 

                    for (int i = 0; i < records.Count; i++)
                    {
                        //string taskId = String.Format("Task{0}", i);
                        //string inputFilename = inputFiles[i].FilePath;
                        //string taskCommandLine = String.Format("cmd /c type {0}", inputFilename);

                        //CloudTask task = new CloudTask(taskId, taskCommandLine);
                        //task.ResourceFiles = new List<ResourceFile> { records[i] };
                        //tasks.Add(task);
                    }

                    // Add all tasks to the job.
                    batchClient.JobOperations.AddTask(JobId, tasks);


                    // Monitor task success/failure, specifying a maximum amount of time to wait for the tasks to complete.

                    TimeSpan timeout = TimeSpan.FromMinutes(30);
                    Console.WriteLine("Monitoring all tasks for 'Completed' state, timeout in {0}...", timeout);

                    IEnumerable<CloudTask> addedTasks = batchClient.JobOperations.ListTasks(JobId);

                    batchClient.Utilities.CreateTaskStateMonitor().WaitAll(addedTasks, TaskState.Completed, timeout);

                    Console.WriteLine("All tasks reached state Completed.");

                    // Print task output
                    Console.WriteLine();
                    Console.WriteLine("Printing task output...");

                    IEnumerable<CloudTask> completedtasks = batchClient.JobOperations.ListTasks(JobId);

                    foreach (CloudTask task in completedtasks)
                    {
                        string nodeId = String.Format(task.ComputeNodeInformation.ComputeNodeId);
                        Console.WriteLine("Task: {0}", task.Id);
                        Console.WriteLine("Node: {0}", nodeId);
                        Console.WriteLine("Standard out:");
                        Console.WriteLine(task.GetNodeFile(Constants.StandardOutFileName).ReadAsString());
                    }

                    // Print out some timing info
                    timer.Stop();
                    Console.WriteLine();
                    Console.WriteLine("Sample end: {0}", DateTime.Now);
                    Console.WriteLine("Elapsed time: {0}", timer.Elapsed);

                    // Clean up Batch resources (if the user so chooses)
                    Console.WriteLine();
                    Console.Write("Delete job? [yes] no: ");
                    string response = Console.ReadLine().ToLower();
                    if (response != "n" && response != "no")
                    {
                        batchClient.JobOperations.DeleteJob(JobId);
                    }

                    Console.Write("Delete pool? [yes] no: ");
                    response = Console.ReadLine().ToLower();
                    if (response != "n" && response != "no")
                    {
                        batchClient.PoolOperations.DeletePool(PoolId);
                    }
                }
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Sample complete, hit ENTER to exit...");
                Console.ReadLine();
            }
        }

        private static void CreateBatchPool(BatchClient batchClient, VirtualMachineConfiguration vmConfiguration)
        {
            try
            {
                CloudPool pool = batchClient.PoolOperations.CreatePool(
                    poolId: PoolId,
                    targetDedicatedComputeNodes: PoolNodeCount,
                    virtualMachineSize: PoolVMSize,
                    virtualMachineConfiguration: vmConfiguration);

                pool.Commit();
            }
            catch (BatchException be)
            {
                // Accept the specific error code PoolExists as that is expected if the pool already exists
                if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                {
                    Console.WriteLine("The pool {0} already existed when we tried to create it", PoolId);
                }
                else
                {
                    throw; // Any other exception is unexpected
                }
            }
        }

        private static VirtualMachineConfiguration CreateVirtualMachineConfiguration(ImageReference imageReference)
        {
            return new VirtualMachineConfiguration(
                imageReference: imageReference,
                nodeAgentSkuId: "batch.node.windows amd64");
        }

        private static ImageReference CreateImageReference()
        {
            return new ImageReference(
                publisher: "MicrosoftWindowsServer",
                offer: "WindowsServer",
                sku: "2016-datacenter-smalldisk",
                version: "latest");
        }
    }
}
