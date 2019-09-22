param([string]$file)
Write-Host "Testing..."
&.\CompilerCLI\bin\Debug\netcoreapp2.1\win10-x64\cheezc.exe $file -o .\gen --stdlib ./examples --run --test --opt