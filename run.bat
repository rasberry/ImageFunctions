: #!/bin/bash
: << 'ENDOFBATCH'

::batch section of script snuck in with the label trick + bash multiline 'comment'

@echo off
setlocal
if "%~1"=="" goto core
findstr /i /r /c:"^[ ]*:%~1" "%~f0" >nul 2>nul
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

:lint
call dotnet format --verify-no-changes
goto :EOF

:make
call dotnet format
if not %ERRORLEVEL%==0 goto :EOF
call dotnet build -consoleloggerparameters:"Summary;Verbosity=normal" -m -p:"WarnLevel=5;EnforceCodeStyleInBuild=true" -t:"clean,build"
if not %ERRORLEVEL%==0 goto :EOF
call dotnet test
goto :EOF

:test
dotnet test -l "console;verbosity=detailed" %*
goto :EOF

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
