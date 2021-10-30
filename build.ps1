param (
    [string]$targetFrameworks = "netcoreapp3.1",
    [string]$configuration = "FAKE_XRM_EASY_9",
    [string]$packTests = ""
 )

$localPackagesFolder = '../local-packages'


Write-Host "Checking if local packages folder '$($localPackagesFolder)' exists..."

$packagesFolderExists = Test-Path $localPackagesFolder -PathType Container

if(!($packagesFolderExists)) 
{
    New-Item $localPackagesFolder -ItemType Directory
}

Write-Host "Deleting previous dependencies..." -ForegroundColor Yellow
$restoredPackagesFolder = './packages'
$restoredPackagesFolderExists = Test-Path $restoredPackagesFolder -PathType Container

if($restoredPackagesFolderExists) 
{
    Get-ChildItem -Path $restoredPackagesFolder -Include fakexrmeasy.* -Directory -Recurse -Force | Remove-Item -Recurse -Force
}
else 
{
    New-Item $restoredPackagesFolder -ItemType Directory
}


if($targetFrameworks -eq "all")
{
    dotnet restore --no-cache --force --force-evaluate /p:Configuration=$configuration /p:PackTests=$packTests --packages $restoredPackagesFolder
}
else {
    dotnet restore --no-cache --force --force-evaluate /p:Configuration=$configuration /p:PackTests=$packTests /p:TargetFrameworks=$targetFrameworks --packages $restoredPackagesFolder
}


if(!($LASTEXITCODE -eq 0)) {
    throw "Error restoring packages"
}

if($targetFrameworks -eq "all")
{
    dotnet build --configuration $configuration --no-restore /p:PackTests=$packTests
}
else 
{
    dotnet build --configuration $configuration --no-restore --framework $targetFrameworks /p:PackTests=$packTests
}

if(!($LASTEXITCODE -eq 0)) {
    throw "Error during build step"
}

if($targetFrameworks -eq "all")
{
    dotnet test --configuration $configuration --no-restore --verbosity normal /p:PackTests=$packTests --collect:"XPlat code coverage" --settings tests/.runsettings --results-directory ./coverage

}
else 
{
    dotnet test --configuration $configuration --no-restore --framework $targetFrameworks --verbosity normal /p:PackTests=$packTests --collect:"XPlat code coverage" --settings tests/.runsettings --results-directory ./coverage
}

if(!($LASTEXITCODE -eq 0)) {
    throw "Error during test step"
}

Write-Host  "*** Build Succeeded :)  **** " -ForegroundColor Green