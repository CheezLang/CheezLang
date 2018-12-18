
$extensionName = "vscodeprefsync"
$vscodeExtDir = "$env:USERPROFILE\.vscode\extensions"

if (Test-Path "$vscodeExtDir\$extensionName") {
    Write-Host "Uninstalling..."
    Remove-Item "$vscodeExtDir\$extensionName" -Force -Recurse
}

Write-Host "Done."