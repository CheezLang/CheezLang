$executable_file_extension = ""
if ($IsLinux) {
    $executable_file_extension = ""
}
if ($IsWindows) {
    $executable_file_extension = ".exe"
}
$configuration = "Release"

$cheezc = "./bin/$configuration/cheezc$executable_file_extension"
