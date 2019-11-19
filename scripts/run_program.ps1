[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file = "test"
)

Push-Location gen
&"./$file$executable_file_extension" -name Nico -age 23
Pop-Location

Write-Host "Program exited with code $LASTEXITCODE"