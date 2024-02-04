param (
    [string]$versionSuffix = "",
    [string]$targetFrameworks = "net6.0",
    [string]$configuration = "FAKE_XRM_EASY_9",
    [string]$projectName = "FakeXrmEasy.Core",
    [string]$projectPath = "src/FakeXrmEasy.Core",
    [string]$packageIdPrefix = "FakeXrmEasy.Core",
    [string]$packTests = ""
 )

Write-Host "Packing configuration for project '$($projectName)' with '$($configuration)' and targetFramework '$($targetFrameworks)' at '$($projectPath)', packTests='$($packTests)'..." -ForegroundColor Yellow

$packageId = $packageIdPrefix;

if($configuration -eq "FAKE_XRM_EASY_9")
{
  $packageId = $('"' + $packageIdPrefix + '.v9"')
}
elseif($configuration -eq "FAKE_XRM_EASY_365")
{
  $packageId = $('"' + $packageIdPrefix + '.v365"')
}
elseif($configuration -eq "FAKE_XRM_EASY_2016")
{
  $packageId = $('"' + $packageIdPrefix + '.v2016"')
}
elseif($configuration -eq "FAKE_XRM_EASY_2015")
{
  $packageId = $('"' + $packageIdPrefix + '.v2015"')
}
elseif($configuration -eq "FAKE_XRM_EASY_2013")
{
  $packageId = $('"' + $packageIdPrefix + '.v2013"')
}
else 
{
  $packageId = $('"' + $packageIdPrefix + '.v2011"')
  Write-Host $packageId
}
$tempNupkgFolder = './nupkgs'

Write-Host "Building..."

./build.ps1 -targetFrameworks $targetFrameworks -configuration $configuration -packTests $packTests

Write-Host "Packing assembly for targetFrameworks $($targetFrameworks)..."
if($targetFrameworks -eq "all")
{
    if($versionSuffix -eq "") 
    {
        dotnet pack --no-build --configuration $configuration -p:PackageID=$packageId -p:Title=$packageId -p:PackTests=$packTests -o $tempNupkgFolder $projectPath/$projectName.csproj
    }
    else {
        dotnet pack --no-build --configuration $configuration -p:PackageID=$packageId -p:Title=$packageId -p:PackTests=$packTests -o $tempNupkgFolder $projectPath/$projectName.csproj --version-suffix $versionSuffix
    }
}
else 
{
    if($versionSuffix -eq "") 
    {
        dotnet pack --no-build --configuration $configuration -p:PackageID=$packageId -p:Title=$packageId -p:PackTests=$packTests -p:TargetFrameworks=$targetFrameworks -o $tempNupkgFolder $projectPath/$projectName.csproj
    }
    else {
        dotnet pack --no-build --configuration $configuration -p:PackageID=$packageId -p:Title=$packageId -p:PackTests=$packTests -p:TargetFrameworks=$targetFrameworks -o $tempNupkgFolder $projectPath/$projectName.csproj --version-suffix $versionSuffix
    }
}


if(!($LASTEXITCODE -eq 0)) {
    throw "Error when packing the assembly for package $($packageIdPrefix) and configuration $($configuration)"
}

Write-Host $("Pack $($packageId) Succeeded :)") -ForegroundColor Green