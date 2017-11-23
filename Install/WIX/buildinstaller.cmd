echo off
color e0
cls
set IS_ELEVATED=0
whoami /groups | find "S-1-16-12288" && set IS_ELEVATED=1
if %IS_ELEVATED%==0 (
	color 84
	echo.
    echo ######### Please run as Admin or start to cry! Exit manually..
	echo.
	goto colorify
)
cls
:isadministrator
echo.
echo ######### Renavigate to current location (Admin rights relocate to C, right?)
echo.
cd /d %~dp0
echo %~dp0

echo.
echo ######### Set some variables 
echo.
set appfolder=GODspeed
set appexe=%appfolder%\Neurotoxin.Godspeed.shell.exe
set appwix=GODspeed
set extensions=-ext WiXNetFxExtension -ext WixBalExtension -ext WixUtilExtension -ext WixUIExtension.dll -ext WixFirewallExtension
set ices=-sice:ICE50 -sice:ICE61 -sice:ICE64
powershell -command "&{[guid]::NewGuid()}" > guidfile
set /p guid=<guidfile
set mainguid=D0C6B22A-726D-4C9A-9FDC-5AA9C22C5FB5
echo Values set. Install guid: %mainguid% (a new generated: %guid%)

echo.
echo ######### Get Assembly versions of %appexe%
echo.
powershell -command "&{[Reflection.Assembly]::Loadfile('%~dp0\%appexe%').GetName().version | %% {write-host ('{0}.{1}.{2}.{3}' -f $_.Major,$_.Minor,$_.Build,$_.Revision)}}" > versionfile
set /p extractedversion=<versionfile
powershell -command "&{[Reflection.Assembly]::Loadfile('%~dp0\%appexe%').GetName().version | %% {write-host ('{0}.{1}' -f $_.Major,$_.Minor)}}" > versionfile
set /p extractedshortversion=<versionfile
echo %extractedversion%


echo.
echo ######### Set config.wxi
echo.
echo ^<?xml version="1.0" encoding="utf-8"?^>^<Include^>^<?define SourceDir = "%~dp0%appfolder%\" ?^>^<?define ProductVersion = "%extractedversion%" ?^>^<?define MainGuid = "%mainguid%" ?^>^</Include^> > config.wxi
echo config.wxi created.

echo.
echo ######### Cleanup
echo.
del versionfile
del guidfile
del /q *.wixobj
del /q *.wixpdb
del /q *.msi
echo Done.

echo.
echo ######### Do the WIXOBJ from WXS
echo.
"WiX Toolset v3.8\bin\candle.exe" %appwix%.wxs %extensions% -dVersion="%extractedversion%" -dShortVersion="%extractedshortversion%"

echo.
echo ######### Do the MSI package
echo.
"WiX Toolset v3.8\bin\light.exe" %ices% %extensions% -o %appwix%.msi %appwix%.wixobj

IF %ERRORLEVEL% EQU 0 goto GoodEnd
echo Errors happened!
pause
goto eof

:GoodEnd
echo.
echo Smile. Its DONE!
pause
goto :EoF


:colorify
FOR %%p IN (8,1,9,2,a,3,b,4,c,5,d,6,e,7,f) DO color 0%%p
goto colorify