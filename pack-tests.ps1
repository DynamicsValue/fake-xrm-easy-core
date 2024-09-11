param (
    [string]$versionSuffix = "",
    [string]$targetFrameworks = "netcoreapp3.1"
 )

Write-Host "Running with versionSuffix '$($versionSuffix)'..."

$tempNupkgFolder = './nupkgs'

$packageIdPrefix = "FakeXrmEasy.CoreTests"
$projectName = "FakeXrmEasy.Core.Tests"
$projectPath = "tests/FakeXrmEasy.Core.Tests"

Write-Host "Packing All Configurations for project $($projectName)" -ForegroundColor Green

./pack-tests-project.ps1 -targetFrameworks $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix

$packageIdPrefix = "FakeXrmEasy.IntegrationTests"
$projectName = "FakeXrmEasy.Integration.Tests"
$projectPath = "tests/FakeXrmEasy.Integration.Tests"

Write-Host "Packing All Configurations for project $($projectName)" -ForegroundColor Green

./pack-tests-project.ps1 -targetFrameworks $targetFrameworks -projectName $projectName -projectPath $projectPath -packageIdPrefix $packageIdPrefix -versionSuffix $versionSuffix

Write-Host "Pack Succeeded  :)" -ForegroundColor Green