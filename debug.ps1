# Debug Helper Script for Docker Compose
# Usage: .\debug.ps1 up|down|restart|logs|rebuild

param(
    [Parameter(Position = 0)]
    [ValidateSet("up", "down", "restart", "logs", "rebuild")]
    [string]$Command = "up"
)

$dockerDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $dockerDir

Write-Host "🐳 Docker Debug Mode Enabled (Debug builds)" -ForegroundColor Green

switch ($Command) {
    "up" {
        Write-Host "Starting containers with Debug configuration..." -ForegroundColor Yellow
        docker-compose up -d
        Write-Host "✅ Containers started" -ForegroundColor Green
        docker-compose logs -f
    }
    "down" {
        Write-Host "Stopping containers..." -ForegroundColor Yellow
        docker-compose down
    }
    "restart" {
        Write-Host "Restarting containers..." -ForegroundColor Yellow
        docker-compose restart
    }
    "logs" {
        docker-compose logs -f
    }
    "rebuild" {
        Write-Host "Rebuilding with Debug configuration..." -ForegroundColor Yellow
        docker-compose up -d --build
    }
}

Pop-Location
