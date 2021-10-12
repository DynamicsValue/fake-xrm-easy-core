param (
    [string]$targetFrameworks = "netcoreapp3.1"
 )

 if($targetFrameworks -eq "netcoreapp3.1")
 {
    ./restore-configuration.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_365"
    ./restore-configuration.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_9"
 }
 else {
    ./restore-configuration.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY"
    ./restore-configuration.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_2013"
    ./restore-configuration.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_2015"
    ./restore-configuration.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_2016"
    ./restore-configuration.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_365"
    ./restore-configuration.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_9"
 }


Write-Host "Restore All Configurations Succeeded  :)" -ForegroundColor Green