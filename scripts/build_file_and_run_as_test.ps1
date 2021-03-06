[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file,

    [Parameter(ValueFromRemainingArguments)]
    [string[]]
    $Passthrough
)


. ./scripts/config.ps1

$name = [System.IO.Path]::GetFileNameWithoutExtension($file)


Write-Host "Testing '$name' ..."
&$cheezc $file --out ./gen/tests --name $name --stdlib "./examples"  --run --test --error-source --print-ast-analysed ./gen/tests/int/$name.chea --emit-llvm-ir @Passthrough

if ($LASTEXITCODE -ne 0) {
    Write-Host "Test '$name' failed." -ForegroundColor Red
} else {
    Write-Host "Test '$name' succeeded." -ForegroundColor Green
}
