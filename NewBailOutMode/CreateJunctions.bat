@echo off
set SourcePath=%~1
set DestPath=%~dp0

if "%SourcePath%" == "" (
	echo Enter the full path (without quotes^) to your Beat Saber game folder 
	set /p SourcePath="Path:"
)

echo Source target: %SourcePath%
echo Link target: %DestPath%
set PluginPath=%SourcePath%\Plugins
set ManagedPath=%SourcePath%\Beat Saber_Data\Managed

if exist "%PluginPath%" (
	echo Plugin folder exists, creating link
	if not exist "%DestPath%References" mkdir "%DestPath%References"
	mklink /J "%DestPath%References\Plugins" "%PluginPath%"
) else (
	echo Plugin folder missing
)
if exist "%ManagedPath%" (
	echo Managed folder exists, creating link
	if not exist "%DestPath%References" mkdir "%DestPath%References"
	mklink /J "%DestPath%References\Managed" "%ManagedPath%"
) else (
	echo Managed folder missing
)

:End
pause