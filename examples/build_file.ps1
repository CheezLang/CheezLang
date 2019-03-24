param([string]$file)
Write-Host "Compiling..."
&.\CompilerCLI\bin\Debug\cheezc.exe $file -o .\gen --stdlib ./examples --print-ast-analysed .\gen\int\ast_analyzed.che --emit-llvm-ir --opt