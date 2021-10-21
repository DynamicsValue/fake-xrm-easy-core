param (
    [string]$packageSource = "local-packages",
    [string]$packagePrefix = "FakeXrmEasy.Abstractions"
 )

Write-Host "Running with packageSource '$($packageSource)'..."

$dirSeparator = [IO.Path]::DirectorySeparatorChar

# Using this glob pattern because of this: https://github.com/dotnet/docs/issues/7146
$tempNupkgFolder = "nupkgs$($dirSeparator)**$($dirSeparator)*.nupkg"

if($packageSource -eq "local-packages") {
    $localPackagesFolder = '../local-packages'
    Write-Host "Deleting previous pushed version '$($localPackagesFolder)'..."
    $projectFilePattern = $packagePrefix + ".*"
    Get-ChildItem -Path $localPackagesFolder -Include $projectFilePattern -File -Recurse | ForEach-Object { $_.Delete()}
}

Write-Host "Pushing '$($packagePrefix)' to source '$($packageSource)' from '$($tempNupkgFolder)'..."

if($packageSource -eq "local-packages") {
    dotnet nuget push $tempNupkgFolder -s $packageSource
}
else 
{
    dotnet nuget push $tempNupkgFolder --skip-duplicate --no-symbols --api-key [System.Environment]::GetEnvironmentVariable('NUGET_TOKEN') -s $packageSource
}

if(!($LASTEXITCODE -eq 0)) {
    throw "Error pushing NuGet package"
}

Write-Host "Push Succeeded :)" -ForegroundColor Green