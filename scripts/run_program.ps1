[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $file = "./gen/test$executable_file_extension"
)

&$file
Write-Host "Program exited with code $LASTEXITCODE"