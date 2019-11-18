[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file = "test"
)

Push-Location gen
&"./$file$executable_file_extension" "-e" "1+2*3"
Pop-Location

Write-Host "Program exited with code $LASTEXITCODE"