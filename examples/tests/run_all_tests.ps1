$failed_tests = 0
$successfull_tests = 0
Get-ChildItem -Path "./examples/tests" -Recurse -Include "*.che" |
Foreach-Object {
    $name = $_.Name.Substring(0, $_.Name.Length - 4)
    Write-Host "Running test $name"

    &.\CompilerCLI\bin\Debug\netcoreapp2.1\win10-x64\cheezc.exe $_.FullName --out .\gen\tests --name $name --run --test --stdlib ./examples

    if ($LASTEXITCODE -ne 0) {
        $failed_tests += 1
    } else {
        $successfull_tests += 1
    }

    Write-Host ""
}

Write-Host ""
Write-Host "$successfull_tests ok. $failed_tests failed."
