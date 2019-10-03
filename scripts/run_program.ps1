[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file = ".\gen\test.exe"
)

&$file
Write-Host "Program exited with code $LASTEXITCODE"