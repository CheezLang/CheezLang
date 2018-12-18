Write-Host "Compiling..."
&.\CompilerCLI\bin\Debug\cheezc.exe .\examples\test.che -o .\gen -n TestProgram
Write-Host "Rendering..."
&.\gen\TestProgram.exe
Write-Host "Done."
&.\test.bmp