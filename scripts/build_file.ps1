[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file
)

. ./scripts/config.ps1

Write-Host "Compiling..."
&$cheezc $file -o ./gen -n test --stdlib ./examples --print-ast-analysed ./gen/int/ast_analyzed.che --emit-llvm-ir --time