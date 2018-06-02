param([string]$target = "x86", [string]$configuration = "Debug")

$targetDir = ""
$targetFileName = "cheez-rt"
$targetTriple = ""

if ($target.Equals("x86")) { 
    $targetDir = "lib\x86"
    $targetTriple = "i386-pc-win32"
} elseif ($target.Equals("x64")) {
    $targetDir = "lib\x64"
    $targetTriple = "x64-pc-win32"
} else {
    Write-Host "Unknown target $target"
    Exit-PSSession
}

if (!(Test-Path -Path "$targetDir")) {
    New-Item -ItemType Directory -Path "$targetDir"
}

if ($configuration.Equals("Debug")) {
    $targetFileName += "d"
}


clang -target $targetTriple -c -o ".\$targetDir\$targetFileName.obj" "src\source.c"

