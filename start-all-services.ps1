Write-Host "🚀 Starting TransitAFC Microservices..." -ForegroundColor Green



Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'Services\User\TransitAFC.Services.User.API'; dotnet run --urls=https://localhost:7001"
Start-Sleep 2

