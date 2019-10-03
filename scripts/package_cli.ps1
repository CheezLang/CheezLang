[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $Runtime = "win10-x64"
)

$targetDir = "./dist/cheez"

function mk-dir {
    param ([string]$path)
    New-Item -Path . -Name $path -ItemType "directory"
}

function copy-stuff {
    param ([string]$path, [string]$targetDir)
    Write-Host "Copying '$path' to '$targetDir'..."

    if (-not (Test-Path $targetDir)) {
        mk-dir $targetDir
    }
    Copy-Item $path $targetDir -Recurse
}

# delete old files if existing
if (Test-Path $targetDir) {
    Write-Host "Deleting old files..."
    Remove-Item $targetDir -Force -Recurse

    sleep 2
}

mk-dir $targetDir

# build compiler in release mode
&dotnet build -o $targetDir -c release -r $Runtime

# binaries
copy-stuff "./lib" $targetDir
copy-stuff "./LLVMLinker.dll" $targetDir

# libraries
copy-stuff "./examples/std" "$targetDir/libraries"
copy-stuff "./examples/bmp/bmp.che" "$targetDir/libraries/bmp"
copy-stuff "./examples/bmp/bmp64.lib" "$targetDir/libraries/bmp"
copy-stuff "./examples/opengl" "$targetDir/libraries"
copy-stuff "./examples/glfw" "$targetDir/libraries"
copy-stuff "./examples/libraries/lua" "$targetDir/libraries"
copy-stuff "./examples/libraries/opencv" "$targetDir/libraries"