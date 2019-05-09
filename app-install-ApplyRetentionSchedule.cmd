@echo off
:: Run Powershell script from a .bat file 
:: http://blog.danskingdom.com/allow-others-to-run-your-powershell-scripts-from-a-batch-file-they-will-love-you-for-it/

set target=%~dpn0.ps1
set arg1=%1
shift
set arg2=%1
shift
set arg3=%1
shift
set arg4=%1
shift
set arg5=%1
shift
set arg6=%1
shift
set arg7=%1
shift
set arg8=%1
shift
set arg9=%1
shift
set arg10=%1
shift
set arg11=%1
shift
set arg12=%1
shift

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '%target%' %arg1% %arg2% %arg3% %arg4% %arg5% %arg6% %arg7% %arg8% %arg9% %arg10% %arg11% %arg12%";