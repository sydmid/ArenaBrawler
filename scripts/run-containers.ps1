<#
.SYNOPSIS
Builds and launches the Arena Brawler game server and Oracle Database container stack.

.DESCRIPTION
This script uses docker-compose to orchestrate the gvenzl/oracle-xe:21-slim database container
and the .NET 9.0 native AOT game server container. It waits for the database to become healthy
before starting the game server.
#>

$ErrorActionPreference = "Stop"

Write-Host "=======================================================" -ForegroundColor Cyan
Write-Host "   Starting Arena Brawler Containerized Infrastructure  " -ForegroundColor Cyan
Write-Host "=======================================================" -ForegroundColor Cyan

# Check if docker-compose is available
if (-not (Get-Command "docker-compose" -ErrorAction SilentlyContinue)) {
    Write-Host "Error: docker-compose is not installed or not in your PATH." -ForegroundColor Red
    Exit 1
}

Write-Host "`n[1/3] Building the .NET Game Server container image..." -ForegroundColor Yellow
docker-compose build

Write-Host "`n[2/3] Starting Oracle Database container (this may take a few minutes)..." -ForegroundColor Yellow
docker-compose up -d oracle-db

Write-Host "Waiting for Oracle DB to become healthy..." -ForegroundColor Yellow
$healthy = $false
for ($i = 0; $i -lt 30; $i++) {
    $status = docker inspect --format="{{.State.Health.Status}}" arena-brawler-db
    if ($status -eq "healthy") {
        $healthy = $true
        break
    }
    Write-Host "Still waiting... ($($i * 10)s)"
    Start-Sleep -Seconds 10
}

if (-not $healthy) {
    Write-Host "Error: Oracle Database failed to become healthy in the expected time." -ForegroundColor Red
    docker-compose logs oracle-db
    Exit 1
}

Write-Host "`n[3/3] Starting the Game Server container..." -ForegroundColor Yellow
docker-compose up -d game-server

Write-Host "`nContainers are running! Streaming logs... (Press Ctrl+C to stop logging, containers will remain running)" -ForegroundColor Green
docker-compose logs -f
