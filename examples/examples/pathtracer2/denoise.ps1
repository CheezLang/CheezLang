[CmdletBinding()]
param (
    [Parameter()]
    [string]
    $filename
)

Push-Location gen
&convert -endian LSB .\$filename .\image.pfm
&"C:\Users\nimao\Downloads\oidn-1.1.0.x64.vc14.windows\oidn-1.1.0.x64.vc14.windows\bin\denoise.exe" -ldr .\image.pfm -o .\out.pfm
&convert -endian LSB .\out.pfm .\image.den.png

Remove-Item image.pfm
Remove-Item out.pfm
Pop-Location