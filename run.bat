: #!/bin/bash
: << 'ENDOFBATCH'

::batch section of script snuck in with the label trick + bash multiline 'comment'

@echo off
setlocal
if "%~1"=="" goto core
findstr /i /r /c:"^[ ]*:%~1\>" "%~f0" >nul 2>nul
if %ERRORLEVEL%==0 echo "running %~1" && call :%* & goto :EOF

::we didn't find a dos label so try running the bash side
:runbash
:: is this wsl or gnuwin32 bash ?
for /f "delims=" %%a in ('bash -c "which wslpath >/dev/null && echo 1"') do set one=%%a

if "%one%"=="1" goto wslbash
:: gnuwin32 bash works fine with dos-y file paths
call bash "%~f0" %*
goto :EOF

:wslbash
:: wslbash requires unix-y paths to the script
for /f "delims=" %%a in ('wsl "wslpath" "%~f0"') do set bp="%%a"
call bash %bp% %*
goto :EOF

:core
::we need all of the plugins in one folder so do a publish
dotnet publish
if not %ERRORLEVEL%==0 goto :EOF
::build\net8.0\publish\ImageFunctions.Core.exe %*
dotnet run --project Core -- %*
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

build\net8.0\publish\ImageFunctions.Core.exe allcolors -# 256 256 -o "test.png" -- -l -on %num%
build\net8.0\publish\ImageFunctions.Core.exe imgdiff -i "test.png" -i "D:\Projects\2020\ImageFunctionsOld\wiki\img\img-11-Default-1.png" -o "diff.png"

set /a num=%num%+256*10
goto cloop

:gui
::we need all of the plugins in one folder so do a publish
::dotnet publish
::if not %ERRORLEVEL%==0 goto :EOF
::build\net8.0\publish\ImageFunctions.Gui.exe %*
dotnet run --project Gui %*
goto :EOF

ENDOFBATCH

function getmnt {
	if [[ "$OSTYPE" == "msys" ]]
		then echo ""
	else
		echo "/mnt"
	fi
}

function _allNames {
	grep -iIrh --include \*.cs -A 2 'GetImageNames' | \
	grep -iPo 'new string\[\].*' | \
	awk -F '[, ]'  '{for (i = 4; i <= NF - 1; i++) {printf "%s\n", $i};}' | \
	tr -d '"'

	ls ./Resources/images | \
	awk -F '.' '{print $1}'
}

function imageallocation {
	_allNames | sort | uniq -ic | sort
}

# if [[ -z "$1" ]]; then usage; exit 1; fi
if [[ -z `declare -F "$1"` ]]; then echo "unknown action '""$1""'"; exit 1; fi

"$1" ${@:2}
