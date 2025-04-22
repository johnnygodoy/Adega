@echo off
cd /d "%~dp0"

echo Iniciando sistema Adega...
start /min Adega.exe --urls "http://localhost:5000"

timeout /t 3 > nul

echo Iniciando middleware WhatsApp...

:: Executa o middleware minimizado com powershell
powershell -WindowStyle Minimized -Command "Start-Process 'node' 'index.js' -WindowStyle Minimized -WorkingDirectory '%~dp0\MiddlewareWhatsapp'"

exit
