param ([bool]$npm_install = $False)

. .\config.ps1

if ($npm_install) {
    Write-Host "Running npm install..."
    &npm install
}


Write-Host "Compiling..."
&npm run compile
if ($LASTEXITCODE -ne 0) {
    Write-Host "Compilation failed, aborting!"
    exit
}

$targetDir = ".\dist\$extensionName"

if (Test-Path "dist") {
    Write-Host "Deleting old files..."
    Remove-Item "dist" -Force -Recurse
}

New-Item -Path . -Name $targetDir -ItemType "directory"

function copy-stuff {
    param ([string]$path)
    Write-Host "Copying '$path' to '$targetDir'..."
    Copy-Item $path $targetDir -Recurse
}

copy-stuff .\out
copy-stuff .\package.json
copy-stuff .\syntaxes
copy-stuff .\language-configuration.json
copy-stuff .\node_modules

Write-Host "Done."