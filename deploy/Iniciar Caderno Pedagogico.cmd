@echo off
cd /d "%~dp0"
start "Caderno Pedagogico" "%~dp0App\SistemaPedagogico.Web.exe" --urls http://127.0.0.1:5080
timeout /t 2 /nobreak >nul
start "" http://127.0.0.1:5080
