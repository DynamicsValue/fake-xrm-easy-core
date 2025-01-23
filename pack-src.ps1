param (
    [string]$versionSuffix = "",
    [string]$targetFrameworks = "net6.0"
 )

Write-Host "Running with versionSuffix '$($versionSuffix)'..."

$tempNupkgFolder = './nupkgs'

$packageIdPrefix = "FakeXrmEasy.Core"
$projectName = "FakeXrmEasy.Core"
$projectPath = "src/FakeXrmEasy.Core"

Write-Host "Packing All Configurations for project $($projectName)" -ForegroundColor Green

./pack-src-project.ps1 -targetFrameworks $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix

Write-Host "Pack Succeeded  :)" -ForegroundColor Green