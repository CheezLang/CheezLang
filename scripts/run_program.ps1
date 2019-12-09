[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file = "test"
)

Push-Location gen
&"./$file$executable_file_extension" find-tokens -file D:\Programming\CheezLang\gen\uiae.che -type StringLiteral
# &"./$file$executable_file_extension" compile -files test.che
# &"./$file$executable_file_extension" project new -name hello_world3 -type program
# &"./$file$executable_file_extension" help
# &"./$file$executable_file_extension" run -file main.che
# &"./$file$executable_file_extension"
Pop-Location

Write-Host "Program exited with code $LASTEXITCODE"