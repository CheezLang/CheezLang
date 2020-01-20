[CmdletBinding()]
param (
    [Parameter()]
    [int]
    $file_count,

    [Parameter()]
    [int]
    $function_count,

    [Parameter()]
    [int]
    $call_count,

    [Parameter()]
    [bool]
    $use_puts
)

$use_puts_string = "false"
if ($use_puts) {
    $use_puts_string = "true"
}

&".\generator\bin\Debug\netcoreapp3.0\generator.exe" $file_count $function_count $call_count $use_puts_string
