.\count_loc.ps1 .\CheezCompiler\Compiler .\CheezCompiler\loc.csv
Write-Host "======================================================"
.\count_loc.ps1 .\CompilerCLI\src .\CompilerCLI\loc.csv
Write-Host "======================================================"
.\count_loc.ps1 .\LanguageServer\VSCodeExtension\src .\LanguageServer\VSCodeExtension\loc.csv *.ts
Write-Host "======================================================"
.\count_loc.ps1 .\LanguageServer\Server .\LanguageServer\Server\loc.csv *.cs TemporaryGeneratedFile*
Write-Host "======================================================"
.\count_loc.ps1 .\examples .\examples\loc.csv *.che