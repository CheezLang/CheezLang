Set-Location gen
&.\test.exe
Set-Location ..
Write-Host "Program exited with code $LASTEXITCODE"