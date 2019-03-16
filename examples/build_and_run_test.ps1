Write-Host "Compiling..."
&.\CompilerCLI\bin\Debug\cheezc.exe .\examples\test.che -o .\gen --stdlib ./examples
Write-Host "Running..."
&.\gen\test.exe
Write-Host "Program exited with code $LASTEXITCODE"