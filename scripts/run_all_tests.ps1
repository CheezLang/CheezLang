[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $dir = "./examples/tests",

    [Parameter(ValueFromRemainingArguments)]
    [string[]]
    $Passthrough
)

$failed_tests = 0
$successfull_tests = 0
Get-ChildItem -Path $dir -Recurse -Include "*.che" |
Foreach-Object {
    ./scripts/build_file_and_run_as_test.ps1 $_.FullName @Passthrough

    if ($LASTEXITCODE -ne 0) {
        $failed_tests += 1
    } else {
        $successfull_tests += 1
    }

    Write-Host ""
}

Write-Host ""
Write-Host "$successfull_tests ok. $failed_tests failed."
