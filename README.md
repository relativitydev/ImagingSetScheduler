# Imaging Set Scheduler

## Overview 

This is the Imaging Set Scheduler application for Relativity. For more information on this application, please visit the Einstein page [here](https://einstein.kcura.com/display/DV/Imaging+Set+Scheduler). Or, you can access the most recent PDF about the Imaging Set Scheduler [here](https://git.kcura.com/projects/IMG/repos/imagingsetscheduler/browse/Documentation/).

The purpose of the Imaging Set Scheduler is to be able to image sets of documents at a pre-determined date and time, in order to increase workflow efficiency for users.

## How to Build

1. Clone the repo in PowerShell:
```sh
git clone https://git@github.com:maher319/ImagingSetScheduler.git
```
2. Reach the Application folder of the ImagingSetScheduler 
3. Build a RAP file using the following command 
```sh
\build_rap.ps1
```
The application schema version will auto bump by 1 each time a build script is run.

## Supported Versions

Imaging Set Scheduler is supported from Relativity 10.3 to Server 2022.
