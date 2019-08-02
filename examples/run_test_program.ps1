Set-Location gen
&.\test.exe test.lox
Set-Location ..
Write-Host "Program exited with code $LASTEXITCODE"