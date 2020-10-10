[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file
)

. ./scripts/config.ps1

Write-Host "Compiling..."
&$cheezc $file -o ./gen -n test --stdlib ./examples --print-ast-analysed ./gen/int/ast_analyzed.chea --print-ast-raw ./gen/int/ast_raw.chea --emit-llvm-ir --time --error-source --print-linker-args --modules ../Libraries/GLFW/glfw