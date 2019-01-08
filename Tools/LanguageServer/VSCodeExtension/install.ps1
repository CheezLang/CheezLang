
. .\config.ps1
$vscodeExtDir = "$env:USERPROFILE\.vscode\extensions"
$distDir = ".\dist\$extensionName" 

if (Test-Path "$vscodeExtDir\$extensionName") {
    Write-Host "Deleting old version..."
    Remove-Item "$vscodeExtDir\$extensionName" -Force -Recurse
}
Write-Host "Copying extension to '$vscodeExtDir'..."
Copy-Item $distDir $vscodeExtDir -Recurse

Write-Host "Done."