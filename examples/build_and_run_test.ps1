.\examples\build_file.ps1 .\examples\test.che

if ($LASTEXITCODE -eq 0) {
    Write-Host "Running..."
    &.\gen\test.exe
    Write-Host "Program exited with code $LASTEXITCODE"
}