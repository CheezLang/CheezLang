. ./scripts/config.ps1
&$cheezc examples/libraries/compiler/debugger.che -o ./gen -n cheezdb --stdlib ./examples --time --error-source --opt
Write-Host "Done"