[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file = "test"
)

Push-Location gen
&"./$file$executable_file_extension" project new -name hello_world -type StaticLibrary
Pop-Location

Write-Host "Program exited with code $LASTEXITCODE"