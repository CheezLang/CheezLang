param([string]$path, [string]$outputFile = "loc.csv", [string]$include = "*.*", [string]$exclude = "")
Clear-Host
"File Name,Lines" | Out-File $outputFile -encoding "UTF8"
Get-ChildItem -re -in $include -ex $exclude $path |
Foreach-Object { 
    $fileStats = Get-Content $_.FullName | Measure-Object -line
    $fileName = $_.Name
    $linesInFile = $fileStats.Lines
    $res = New-Object -TypeName PSObject
    $res | Add-Member -MemberType NoteProperty -Name FileName -Value $fileName
    $res | Add-Member -MemberType NoteProperty -Name Lines -Value $linesInFile
    $res
} | Sort-Object Lines -Descending | ForEach-Object {
    $fileName = $_.FileName
    $lines = $_.Lines
    $formatted = "{0,25}: {1}" -f $fileName,$lines
    Write-Host $formatted
    "$fileName,$lines"
} | Out-File $outputFile -Append -encoding "UTF8"
