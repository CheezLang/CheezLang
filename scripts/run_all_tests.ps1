[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $dir = "./examples/tests",

    [Parameter(ValueFromRemainingArguments)]
    [string[]]
    $Passthrough
)

$failed_tests = New-Object System.Collections.ArrayList($null)
$successfull_tests = New-Object System.Collections.ArrayList($null)

Get-ChildItem -Path $dir -Recurse -Include "*.che" | Foreach-Object {
    ./scripts/build_file_and_run_as_test.ps1 $_.FullName @Passthrough

    if ($LASTEXITCODE -ne 0) {
        [void]($failed_tests.Add($_.FullName))
    } else {
        [void]($successfull_tests.Add($_.FullName))
    }

    Write-Host ""
}

$num_of_ok_tests = $successfull_tests.count
$num_of_bad_tests = $failed_tests.count

Write-Host ""
Write-Host "$num_of_ok_tests ok. $num_of_bad_tests failed."

Write-Host "Failed tests:"
foreach ($item in $failed_tests) {
    Write-Host $item
}
