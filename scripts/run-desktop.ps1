<#
.SYNOPSIS
    Builds and starts the Flowery.NET.Gallery.Desktop application.

.DESCRIPTION
    Builds the Desktop project and, if the resulting .exe exists, starts it in the background.

.PARAMETER Configuration
    Build configuration to use (default: Debug).

.EXAMPLE
    pwsh ./scripts/run-desktop.ps1
    pwsh ./scripts/run-desktop.ps1 -Configuration Release
#>
param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$desktopProject = Join-Path $repoRoot "Flowery.NET.Gallery.Desktop/Flowery.NET.Gallery.Desktop.csproj"

if (-not (Test-Path $desktopProject)) {
    Write-Host "ERROR: Desktop project not found at $desktopProject" -ForegroundColor Red
    exit 1
}

Write-Host "Building: Flowery.NET.Gallery.Desktop" -ForegroundColor Cyan
Write-Host "  Configuration: $Configuration" -ForegroundColor Gray

dotnet build "$desktopProject" -c $Configuration

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

[xml]$desktopProjectXml = Get-Content -Raw $desktopProject
$tfm = ($desktopProjectXml.Project.PropertyGroup | Where-Object { $_.TargetFramework } | Select-Object -First 1).TargetFramework

$projectName = [System.IO.Path]::GetFileNameWithoutExtension($desktopProject)
$desktopOutput = Join-Path $repoRoot "bin/$Configuration/$projectName/$tfm"
$exePath = Join-Path $desktopOutput "$projectName.exe"

if (-not (Test-Path $exePath)) {
    Write-Host "Desktop executable not found at $exePath (skipping run)" -ForegroundColor Yellow
    exit 0
}

Write-Host "Starting Desktop app in background..." -ForegroundColor Cyan
Start-Process -FilePath $exePath -WorkingDirectory $desktopOutput | Out-Null
Write-Host "Started: $exePath" -ForegroundColor Green
