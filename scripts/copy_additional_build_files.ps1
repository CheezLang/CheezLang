[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $solution_dir,

    [Parameter()]
    [string]
    $project_dir,

    [Parameter()]
    [string]
    $out_dir
)

$solution_dir = $solution_dir.Replace("\\", "/")
$project_dir = $project_dir.Replace("\\", "/")
$out_dir = $out_dir.Replace("\\", "/")

Write-Output "copy additional files to $project_dir/$out_dir"

Copy-Item "$solution_dir/modules*.txt" -Destination "$project_dir/$out_dir" -Force
Copy-Item "$solution_dir/lib" -Destination "$project_dir/$out_dir" -Recurse -Force
