param([string]$path, [string]$outputFile = "loc.csv", [string[]]$include = "*.*", [string]$exclude = "")
Write-Host "Counting lines in" $path
if ($exclude) { Write-Host "Excluding:" $exclude }
Write-Host ""
$totalLineCount = 0
$totalFileCount = 0
"File Name,Lines" | Out-File $outputFile -encoding "UTF8"
Get-ChildItem -re -in $include -Exclude $exclude $path |
Foreach-Object { 
    $fileStats = Get-Content $_.FullName | Measure-Object -line
    $fileName = $_.Name
    $linesInFile = $fileStats.Lines
    $totalLineCount += $linesInFile
    $totalFileCount += 1
    $res = New-Object -TypeName PSObject
    $res | Add-Member -MemberType NoteProperty -Name FileName -Value $fileName
    $res | Add-Member -MemberType NoteProperty -Name Lines -Value $linesInFile
    $res
} | Sort-Object Lines -Descending | ForEach-Object {
    $fileName = $_.FileName
    $lines = $_.Lines
    $formatted = "{0,40}: {1}" -f $fileName,$lines
    Write-Host $formatted
    "$fileName,$lines"
} | Out-File $outputFile -Append -encoding "UTF8"

"Total,$totalLineCount" | Out-File $outputFile  -Append -encoding "UTF8"

Write-Host ""
$totalLineCountStr = "{0,40}: {1} lines in {2} file(s)" -f "Total",$totalLineCount,$totalFileCount
Write-Host $totalLineCountStr
