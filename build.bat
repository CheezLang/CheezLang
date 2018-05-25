@echo off

call "D:\Utilities\llc.exe" -O0 -filetype=obj -o %1.obj %1.ll

rem setup environment variables for assembler and linker
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat"

rem compile io lib
rem call ml /nologo /c /W3 /errorReport:none /Fo"io-x86.obj" "..\io-x86.asm"

rem link files
call link.exe /OUT:%1.exe "-libpath:C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\VC\\Tools\\MSVC\\14.12.25827\\lib\\x86" "-libpath:C:\\Program Files (x86)\\Windows Kits\\10\\Lib\\10.0.16299.0\\ucrt\\x86" "-libpath:C:\\Program Files (x86)\\Windows Kits\\10\\Lib\\10.0.16299.0\\um\\x86" /NOLOGO /ENTRY:main /DEBUG:NONE /MACHINE:X86 /SUBSYSTEM:CONSOLE /ERRORREPORT:NONE kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib "..\Debug\cheezio.lib" %1.obj
rem call lld-link "..\Debug\cheezio.lib" %1.obj
if not %errorLevel% == 0 goto :linkerFailed

:success
goto :end

:compilerFailed
echo Compilation failed with code %errorlevel%
pause
rem exit
goto :end

:assemblerFailed
echo Assembler failed with code %errorlevel%
pause
rem exit
goto :end

:linkerFailed
echo Linker failed with code %errorlevel%
pause
rem exit
goto :end

:end

rem exit