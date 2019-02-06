# Steps for the Workload

1) Create a Resource Group
2) Create a Batch Account
3) Create an Application
4) Create a Pool [with fixed nodes size and auto scaling option based on formula]
5) Create a Job
6) Create a Task
7) Configure a database on premise [Cassandra] 
8) Connect with local database from Batch Task
9) Send the email

# Problem Statement:

We have 100,000 users in our database in an on premise environment and the requirement is to create a Batch which will read some attributes from the user database. It will then create PDF files customized for that user and send it through the mail.

# Things to figure out

## Trigger Point: How the job will be triggered

Is it going to be something from on premise or on cloud?

## Establish connection between on premise to server for database connection

## Basic Interaction

![Basic Interaction](https://github.com/vishaldwivedi/Azure_EmailWorkloadBatch/blob/master/Azure%20Batch.png)



