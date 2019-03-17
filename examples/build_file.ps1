param([string]$file)
Write-Host "Compiling..."
# &.\CompilerCLI\bin\Debug\cheezc.exe $file
&.\CompilerCLI\bin\Debug\cheezc.exe $file -o .\gen --stdlib ./examples --opt