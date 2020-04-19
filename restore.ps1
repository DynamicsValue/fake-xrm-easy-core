$localPackageRestoreFolder = './packages'
Write-Host "Checking if local packages folder '$($localPackageRestoreFolder)' exists..."

$packageRestoreFolderExists = Test-Path $localPackageRestoreFolder -PathType Container

if(!($packageRestoreFolderExists)) 
{
    New-Item $localPackageRestoreFolder -ItemType Directory
}

Write-Host "Deleting previous installed packages in '$($localPackageRestoreFolder)'..."
Remove-Item -Recurse -Force $localPackageRestoreFolder

Write-Host "Restoring packages..."
dotnet restore 
if(!($LASTEXITCODE -eq 0)) {
    throw "Error restoring packages"
}

Write-Host "Done. It's convenient to restart vscode after this." -ForegroundColor Green