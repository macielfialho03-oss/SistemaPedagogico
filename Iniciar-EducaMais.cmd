@echo off
chcp 65001 >nul
cd /d "%~dp0"
dotnet --list-sdks | findstr /b "10." >nul
if errorlevel 1 (
 echo Instale o SDK do .NET 10 para Windows x64.
 echo https://dotnet.microsoft.com/download/dotnet/10.0
 pause
 exit /b 1
)
echo EducaMais: abra http://localhost:5003 quando aparecer "Now listening on".
echo Deixe esta janela aberta. Para encerrar, pressione Ctrl+C.
dotnet run --project src\SistemaPedagogico.Web --launch-profile http
if errorlevel 1 pause
