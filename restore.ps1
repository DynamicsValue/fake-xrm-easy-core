$localPackageRestoreFolder = './packages'
Write-Host "Checking if local packages folder '$($localPackageRestoreFolder)' exists..."

$packageRestoreFolderExists = Test-Path $localPackageRestoreFolder -PathType Container

if(!($packageRestoreFolderExists)) 
{
    New-Item $localPackageRestoreFolder -ItemType Directory
}

Write-Host "Deleting previous installed packages..."
Get-ChildItem -Path $packageRestoreFolderExists -Include *.* -File -Recurse | ForEach-Object { $_.Delete()}

Write-Host "Restoring packages..."
dotnet restore 

Write-Host "Done" -ForegroundColor Green