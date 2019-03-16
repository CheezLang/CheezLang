param([string]$name, [string]$category = "language")

Write-Host "Running test $name"
&.\CompilerCLI\bin\Debug\cheezc.exe "./examples/tests/$category/$name.che" --out .\gen\tests --name $name --run --test --stdlib "./examples"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Test $name failed."
} else {
    Write-Host "Test $name successfull."
}