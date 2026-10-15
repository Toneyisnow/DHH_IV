@echo off
set "UNITY=C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"
if not exist "%UNITY%" (
 echo Unity 6000.3.10f1 was not found. Open this folder from Unity Hub.
 pause
 exit /b 1
)
start "" "%UNITY%" -projectPath "%~dp0."
