# Problem Statement:

We have 100,000 users in our database in an on premise environment and the requirement is to create a Batch which will read some attributes from the user database. It will then create PDF files customized for that user and send it through the mail.

# Steps for the Workload

1) Create a Resource Group [Currently being done in Azure Portal Directly]
2) Create a Batch Account [Currently being done in Azure Portal Directly]
3) Create an Application
4) Create a Pool [with fixed nodes size and auto scaling option based on formula]
5) Create a Job
6) Create a Task
7) Configure a database on premise [Cassandra] 
8) Connect with local database from Batch Task
9) Send the email

# Things to figure out

## Trigger Point: How the job will be triggered

### Consideration 1

Use Azure Function to trigger the batch. The Azure functions have a time limit of 10 minutes maximum to eecute so not sure if that will be correct way.

### Consideration 2

Use Azure Durable Function [Need to evaluate approach here]

https://docs.microsoft.com/en-us/azure/batch/quick-run-dotnet

Is it going to be something from on premise or on cloud?

## Establish connection between on premise to server for database connection

## Basic Interaction

![Basic Interaction](https://github.com/vishaldwivedi/Azure_EmailWorkloadBatch/blob/master/Azure%20Batch.png)

## High Availability

This application is not being designed to provide High availability and managing failover. To provide high availability in the future following points could be considered:

1) Multiple Batch accounts in multiple regions
2) Pre-create all required accounts in each region, such as the Batch account and storage account. There often is not any charge for having accounts created, only when there is data stored or the account is used.
Make sure quotas are set on the accounts ahead of time, so you can allocate the required number of cores using the Batch account.
3) Use templates and/or scripts to automate the deployment of the application in a region.
4) Keep application binaries and reference data up-to-date in all regions. Staying up-to-date will ensure the region can be brought online quickly without having to wait for the upload and deployment of files. For example, if a custom application to install on pool nodes is stored and referenced using Batch application packages, then when a new version of the application is produced, it should be uploaded to each Batch account and referenced by the pool configuration (or make the new version the default version).
5) In the application calling Batch, storage, and any other services, easily switchover clients or the load to the different region.
6) A best practice to ensure a failover will be successful is to frequently switchover to an alternate region as part of normal operation. For example, with two deployments in separate regions, switchover to the alternate region every month.



