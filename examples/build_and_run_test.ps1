Write-Host "Compiling..."
&.\CompilerCLI\bin\Debug\cheezc.exe .\examples\test.che -o .\gen -n TestProgram
Write-Host "Running..."
&.\gen\TestProgram.exe