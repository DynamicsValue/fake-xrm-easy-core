param (
    [string]$versionSuffix = "",
    [string]$targetFrameworks = "netcoreapp3.1",
    [string]$packageIdPrefix = "",
    [string]$projectName = "",
    [string]$projectPath = ""   
 )

Write-Host "Running with versionSuffix '$($versionSuffix)'..."

$tempNupkgFolder = './nupkgs'

Write-Host "Packing All Configurations for project $($projectName)" -ForegroundColor Green

 if($targetFrameworks -eq "netcoreapp3.1")
 {
    ./pack-configuration.ps1 -targetFramework $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -configuration "FAKE_XRM_EASY_365" 
    ./pack-configuration.ps1 -targetFramework $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -configuration "FAKE_XRM_EASY_9" 
 }
 else {
    ./pack-configuration.ps1 -targetFramework $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -configuration "FAKE_XRM_EASY" 
    ./pack-configuration.ps1 -targetFramework $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -configuration "FAKE_XRM_EASY_2013"
    ./pack-configuration.ps1 -targetFramework $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -configuration "FAKE_XRM_EASY_2015"
    ./pack-configuration.ps1 -targetFramework $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -configuration "FAKE_XRM_EASY_2016"
    ./pack-configuration.ps1 -targetFramework $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -configuration "FAKE_XRM_EASY_365"
    ./pack-configuration.ps1 -targetFramework $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix -configuration "FAKE_XRM_EASY_9"
 }

Write-Host "Pack Succeeded  :)" -ForegroundColor Green