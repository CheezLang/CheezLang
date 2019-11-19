[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file = "test"
)

./scripts/build_file.ps1 "./examples/$file.che"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Running..."
    ./scripts/run_program.ps1 $file
}