@echo off
chcp 65001 >nul
cd /d "%~dp0"
dotnet test SistemaPedagogico.slnx > resultado-testes.txt 2>&1
type resultado-testes.txt
pause
