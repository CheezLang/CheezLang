param([string]$name)

Write-Host "Running test $name"
&.\CompilerCLI\bin\Debug\cheezc.exe "./examples/tests/language/$name" --out .\gen\tests --name $name --run --test

if ($LASTEXITCODE -ne 0) {
    Write-Host "Test $name failed."
} else {
    Write-Host "Test $name successfull."
}
