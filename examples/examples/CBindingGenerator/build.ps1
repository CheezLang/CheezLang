[CmdletBinding()]
param ([Parameter(ValueFromRemainingArguments)] [string[]] $Passthrough)
&cheezc ./src/main.che --out ./bin --name CBindingGenerator --time @Passthrough