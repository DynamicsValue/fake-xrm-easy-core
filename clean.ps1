param (
    [string]$folderPath = "./src/FakeXrmEasy.Core/bin"
)

if (Test-Path -Path $folderPath) {
  Get-ChildItem -Path $folderPath -Include * -File -Recurse | foreach { $_.Delete()}
}
