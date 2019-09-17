param([string]$file)
Write-Host "Compiling..."
&.\CompilerCLI\bin\Debug\netcoreapp2.1\win10-x64\cheezc.exe $file -o .\gen --stdlib ./examples --print-ast-analysed .\gen\int\ast_analyzed.che --emit-llvm-ir --time --opt