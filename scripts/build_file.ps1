[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file
)

. ./scripts/config.ps1

Write-Host "Compiling..."
&$cheezc $file -o ./gen -n test --stdlib ./examples --print-ast-analysed ./gen/int/ast_analyzed.che --print-ast-raw ./gen/int/ast_raw.che --emit-llvm-ir --time