@echo off
dotnet publish
if not %ERRORLEVEL%==0 goto :EOF
::call dotnet run --project src -- %*
build\net7.0\publish\core.exe %*