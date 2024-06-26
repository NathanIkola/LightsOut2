@echo off
echo %cd%

:: Delete the existing assemblies
rmdir "../../Assemblies" /S /Q

:: Copy the newly-built assemblies
mkdir "../../Assemblies"
echo f | xcopy /f /Y "LightsOut2.dll" "../../Assemblies/LightsOut2.dll"
echo f | xcopy /f /Y "LightsOut2.Core.dll" "../../Assemblies/LightsOut2.Core.dll"
echo f | xcopy /f /Y "LightsOut2.ModCompatibility.dll" "../../Assemblies/LightsOut2.ModCompatibility.dll"

:: Pop up to the Source directory and execute the custom post-build if it's present
cd ..
if exist "post-build-custom.cmd" ( call post-build-custom.cmd )