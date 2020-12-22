param (
    [string]$versionSuffix = "",
    [string]$targetFrameworks = "netcoreapp3.1"
 )

$project = "FakeXrmEasy.Core"

Write-Host "Running with versionSuffix '$($versionSuffix)'..."

$tempNupkgFolder = './nupkgs'

Write-Host "Checking if temp nupkgs folder '$($tempNupkgFolder)' exists..."
$tempNupkgFolderExists = Test-Path $tempNupkgFolder -PathType Container
if(!($tempNupkgFolderExists)) 
{
    New-Item $tempNupkgFolder -ItemType Directory
}

Write-Host "Deleting temporary nupkgs..."
Get-ChildItem -Path $tempNupkgFolder -Include *.nupkg -File -Recurse | ForEach-Object { $_.Delete()}

Write-Host "Packing assembly for targetFrameworks $($targetFrameworks)..."
if($targetFrameworks -eq "all")
{
    if($versionSuffix -eq "") 
    {
        dotnet pack -o $tempNupkgFolder src/$project/$project.csproj
    }
    else {
        dotnet pack -o $tempNupkgFolder src/$project/$project.csproj --version-suffix $versionSuffix
    }
}
else 
{
    if($versionSuffix -eq "") 
    {
        dotnet pack -p:TargetFrameworks=$targetFrameworks -o $tempNupkgFolder src/$project/$project.csproj
    }
    else {
        dotnet pack -p:TargetFrameworks=$targetFrameworks -o $tempNupkgFolder src/$project/$project.csproj --version-suffix $versionSuffix
    }
}
if(!($LASTEXITCODE -eq 0)) {
    throw "Error when packing the assembly"
}

Write-Host "Pack Succeeded :)" -ForegroundColor Green