[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file = "test"
)

./scripts/build_file.ps1 "./examples/$file.che"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Running..."
    Push-Location gen
    &"./$file.exe"
    Pop-Location
    Write-Host "Program exited with code $LASTEXITCODE"
}