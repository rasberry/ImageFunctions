@echo off
setlocal
if not "%~1"=="" call :%~1 && goto :EOF
if not %ERRORLEVEL%==0 goto :EOF


dotnet publish
if not %ERRORLEVEL%==0 goto :EOF
::call dotnet run --project src -- %*
build\net7.0\publish\ImageFunctions.Core.exe %*
goto :EOF

:wiki
dotnet run --project Writer
goto :EOF

:test
dotnet test -l "console;verbosity=detailed" %*
goto :EOF

:: Test allcolors for backwards compatability offset
:ctest
set /a num=128*%~2

:cloop
::if %num% GEQ 0 goto :EOF
echo %num%

build\net7.0\publish\ImageFunctions.Core.exe allcolors -# 256 256 -o "test.png" -- -l -on %num%
build\net7.0\publish\ImageFunctions.Core.exe imgdiff -i "test.png" -i "D:\Projects\2020\ImageFunctionsOld\wiki\img\img-11-Default-1.png" -o "diff.png"

set /a num=%num%+256*10
goto cloop