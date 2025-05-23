@echo off
echo Starting URL Shortener services...

:: Start API in a new window
start cmd /k "dotnet run --project URLShortener.API/URLShortener.API.csproj --launch-profile https"

:: Give API time to start
echo Waiting for API to start...
timeout /t 5 /nobreak > nul

:: Start UI in a new window
start cmd /k "dotnet run --project URLShortener.UI/URLShortener.UI.csproj --launch-profile https"

echo Services started!
echo API URL: https://localhost:7148
echo UI URL: https://localhost:7161 