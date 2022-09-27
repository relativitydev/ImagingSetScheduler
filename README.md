# Imaging Set Scheduler

## Overview 

This is the Imaging Set Scheduler application for Relativity. Imaging Set Scheduler will create sets of documents to be imaged. A custom object at the workspace level will be used to define the documents to be imaged and when. A Relativity Agent will run in the environment that sends documents to the imaging queue.
You can access the most recent documentation about the Imaging Set Scheduler [here](https://github.com/relativitydev/ImagingSetScheduler/tree/master/Documentation).

The purpose of the Imaging Set Scheduler is to be able to image sets of documents at a pre-determined date and time, in order to increase workflow efficiency for users.

## How to Build

1. Clone the repo
2. Reach the Application folder of the ImagingSetScheduler 
3. Build a RAP file using the following command 
```sh
\build_rap.ps1
```
The application schema version will auto bump by 1 each time a build script is run.

## Supported Versions

Imaging Set Scheduler is supported from Relativity 10.3 to Server 2022.
