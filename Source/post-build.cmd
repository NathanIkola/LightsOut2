@echo off
echo %cd%

:: Delete the existing assemblies
rmdir "../../../Assemblies" /S /Q

:: Copy the newly-built assemblies
mkdir "../../../Assemblies"
echo f | xcopy /f /Y "bin/Debug/net10.0/LightsOut2.dll" "../../../Assemblies/LightsOut2.dll"
echo f | xcopy /f /Y "bin/Debug/net10.0/LightsOut2.Core.dll" "../../../Assemblies/LightsOut2.Core.dll"
echo f | xcopy /f /Y "bin/Debug/net10.0/jl08lib.dll" "../../../Assemblies/jl08lib.dll"
:: Copy portable symbols so VS Code can bind breakpoints in the deployed mod
echo f | xcopy /f /Y "bin/Debug/net10.0/LightsOut2.pdb" "../../../Assemblies/LightsOut2.pdb"
echo f | xcopy /f /Y "bin/Debug/net10.0/LightsOut2.Core.pdb" "../../../Assemblies/LightsOut2.Core.pdb"
echo f | xcopy /f /Y "bin/Debug/net10.0/jl08lib.pdb" "../../../Assemblies/jl08lib.pdb"
