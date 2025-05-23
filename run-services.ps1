# Run both API and UI services
Write-Host "Starting URL Shortener services..." -ForegroundColor Green

# Start API in a new window
Start-Process -FilePath "dotnet" -ArgumentList "run --project URLShortener.API/URLShortener.API.csproj --launch-profile https" -WorkingDirectory (Get-Location)

# Give API time to start
Write-Host "Waiting for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Start UI in a new window
Start-Process -FilePath "dotnet" -ArgumentList "run --project URLShortener.UI/URLShortener.UI.csproj --launch-profile https" -WorkingDirectory (Get-Location)

Write-Host "Services started!" -ForegroundColor Green
Write-Host "API URL: https://localhost:7148" -ForegroundColor Cyan
Write-Host "UI URL: https://localhost:7161" -ForegroundColor Cyan 