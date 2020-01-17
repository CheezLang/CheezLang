[CmdletBinding()]
param (
    [Parameter()]
    [bool]
    $use_ps_measure
)

if (-not (Test-Path "./out/main.che")) {
    Write-Host "No main file was found. Please create a main file in ./out/main.che"
    Exit-PSSession
}

Push-Location "./out"
if ($use_ps_measure) {
    Measure-Command { &cheezc.exe "./main.che" --time --out "../bin" }
} else {
    &cheezc.exe "./main.che" --time --out "../bin"
}
Pop-Location
