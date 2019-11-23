[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file = "test"
)

Push-Location gen
# &"./$file$executable_file_extension" compile -files test.che
# &"./$file$executable_file_extension" project new -name hello_world -type Program
&"./$file$executable_file_extension"
Pop-Location

Write-Host "Program exited with code $LASTEXITCODE"