param (
    [string]$targetFrameworks = "netcoreapp3.1"
 )

 if($targetFrameworks -eq "netcoreapp3.1")
 {
    ./build.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_365"
    ./build.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_9"
 }
 else {
    ./build.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY"
    ./build.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_2013"
    ./build.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_2015"
    ./build.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_2016"
    ./build.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_365"
    ./build.ps1 -targetFramework $targetFrameworks -configuration "FAKE_XRM_EASY_9"
 }


Write-Host "Build All Configurations Succeeded  :)" -ForegroundColor Green