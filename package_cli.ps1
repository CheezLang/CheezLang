param([string]$built_binaries)

$targetDir = ".\dist\cheez"

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

if (Test-Path $targetDir) {
    Write-Host "Deleting old files..."
    Remove-Item $targetDir -Force -Recurse
}

mk-dir $targetDir

# binaries
copy-stuff ".\lib" $targetDir
copy-stuff ".\Linker.dll" $targetDir
copy-stuff ".\CompilerCLI\bin\Release\*" $targetDir

# libraries
copy-stuff ".\examples\std" "$targetDir\libraries"
copy-stuff ".\examples\bmp\bmp.che" "$targetDir\libraries\bmp"
copy-stuff ".\examples\bmp\bmp.lib" "$targetDir\libraries\bmp"
copy-stuff ".\examples\opengl" "$targetDir\libraries"
copy-stuff ".\examples\glfw" "$targetDir\libraries"
