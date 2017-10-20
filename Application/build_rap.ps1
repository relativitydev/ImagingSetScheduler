# This will build a new RAP and increment the minor (last) version number.
# If a major version number (or the app) is modified place new app.xml in this directory
# Usage: .\build_rap.ps1 Debug [or Release]
param(
    [string]$build
)
Add-Type -assembly "system.io.compression.filesystem"
Remove-Item RA_Imaging_Set_Scheduler.rap
$xml=New-Object XML
$xml.Load("$pwd\application.xml")
$node = $xml.SelectSingleNode("/Application/Version")
$currentVersion = $node.InnerText
[array]$versionNumbers = $currentVersion.Split(".")
$minorNumber = [convert]::ToInt32($versionNumbers[3],10)
$minorNumber++
$versionNumbers[3] = [convert]::ToString($minorNumber)
$node.InnerText = $versionNumbers -join '.'
$xml.Save("$pwd\application.xml")
$dllPath = "..\Source\Code\KCD_1041539.ImagingSetScheduler\KCD_1041539.ImagingSetScheduler\bin\" + $build + "\"
$appDll = $dllpath + "KCD_1041539.ImagingSetScheduler.dll"
$imagingDlls = $dllpath + "Relativity.Imaging.Services.Interfaces.dll"
New-Item rapFolder -type directory
New-Item rapFolder\assemblies -type directory
Copy-Item $appDll rapFolder\assemblies
Copy-Item $imagingDlls rapFolder\assemblies
Copy-Item application.xml rapFolder
[io.compression.zipfile]::CreateFromDirectory("$pwd\rapFolder", "$pwd\RA_Imaging_Set_Scheduler.zip")
Remove-Item rapFolder -recurse
Rename-Item RA_Imaging_Set_Scheduler.zip RA_Imaging_Set_Scheduler.rap