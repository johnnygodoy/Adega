@echo off
cd /d "%~dp0"

REM Iniciar sistema principal
echo Iniciando sistema Adega...
start /min Adega.exe --urls "http://localhost:5000"

REM Iniciar middleware WhatsApp
echo Iniciando integração com WhatsApp...
cd MiddlewareWhatsapp
start cmd /k "node index.js"
