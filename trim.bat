@echo off

call :go "D:\temp\bordertrim\pie059-c.webp"
call :go "D:\temp\bordertrim\pie059.webp"
call :go "D:\temp\bordertrim\pie060-c.webp"
call :go "D:\temp\bordertrim\pie060.webp"
call :go "D:\temp\bordertrim\pie338-c.webp"
call :go "D:\temp\bordertrim\pie338.webp"
call :go "D:\temp\bordertrim\pie339-c.webp"
call :go "D:\temp\bordertrim\pie339.webp"
call :go "D:\temp\bordertrim\pie340-c.webp"
call :go "D:\temp\bordertrim\pie340.webp"
goto :EOF

:go
if exist "trim-%~n1.png" goto :EOF
:: call run cli trimedge -i "%~1" -- -f "3%%%%%%%%"
call fmw trim2detail "%~1" "trim-%~n1.png"

goto :EOF