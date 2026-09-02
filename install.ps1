# ALX Installer — Windows
# Run this script to install or upgrade ALX on your system
# Usage: .\install.ps1
# Upgrade: .\install.ps1 (auto-detects existing installation)

param(
    [string]$InstallDir = "$env:LOCALAPPDATA\ALX",
    [switch]$Force
)

$IsUpgrade = Test-Path "$InstallDir\ALX.CLI.exe"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ALX - Alexion Language Installer" -ForegroundColor Cyan
if ($IsUpgrade) {
    $InstalledVersion = (& "$InstallDir\ALX.CLI.exe" version 2>&1 | Select-String "ALX" | Select-Object -First 1).ToString().Trim()
    Write-Host "  Detected: $InstalledVersion" -ForegroundColor Yellow
    Write-Host "  Upgrading to latest..." -ForegroundColor Yellow
} else {
    Write-Host "  Fresh installation" -ForegroundColor Green
}
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build ALX
Write-Host "[1/4] Building ALX..." -ForegroundColor Yellow
dotnet build ALX.sln -c Release --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Make sure .NET 8 SDK is installed." -ForegroundColor Red
    Write-Host "Download: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Red
    exit 1
}
Write-Host "  Build successful!" -ForegroundColor Green

# Step 2: Publish CLI
Write-Host "[2/4] Publishing ALX CLI..." -ForegroundColor Yellow

# For upgrade: backup existing examples if user modified them
$BackupDir = ""
if ($IsUpgrade -and (Test-Path "$InstallDir\examples")) {
    $BackupDir = Join-Path $env:TEMP "alx-backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Copy-Item -Recurse "$InstallDir\examples" $BackupDir -Force
    Write-Host "  Backed up existing examples to: $BackupDir" -ForegroundColor Gray
}

# Clean and recreate install dir
if (Test-Path $InstallDir) { Remove-Item -Recurse -Force $InstallDir }
New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null

dotnet publish src/ALX.CLI -c Release -r win-x64 --self-contained false -o $InstallDir --verbosity quiet

# Copy examples and docs
if (Test-Path "examples") { Copy-Item -Recurse -Force "examples" "$InstallDir\examples" }
if (Test-Path "docs\site") { Copy-Item -Recurse -Force "docs\site" "$InstallDir\docs" }

# Create alx.bat launcher
$batContent = "@echo off`r`n`"%~dp0ALX.CLI.exe`" %*"
Set-Content -Path "$InstallDir\alx.bat" -Value $batContent -Encoding ASCII

# Copy VS Code extension files
if (Test-Path "syntaxes") { Copy-Item -Recurse -Force "syntaxes" "$InstallDir\vscode-ext\syntaxes" }
if (Test-Path "package.json") { Copy-Item -Force "package.json" "$InstallDir\vscode-ext\package.json" }
if (Test-Path "language-configuration.json") { Copy-Item -Force "language-configuration.json" "$InstallDir\vscode-ext\language-configuration.json" }

Write-Host "  Published to: $InstallDir" -ForegroundColor Green

# Step 3: Add to PATH
Write-Host "[3/4] Checking PATH..." -ForegroundColor Yellow

$currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
$pathEntries = $currentPath -split ";" | Where-Object { $_.Trim() -ne "" }

if ($pathEntries -contains $InstallDir) {
    Write-Host "  ALX is already in PATH!" -ForegroundColor Green
} else {
    $newPath = ($pathEntries + $InstallDir) -join ";"
    [Environment]::SetEnvironmentVariable("Path", $newPath, "User")
    Write-Host "  Added to PATH: $InstallDir" -ForegroundColor Green
    Write-Host "  (Restart your terminal for PATH changes to take effect)" -ForegroundColor Yellow
}

# Step 4: Verify
Write-Host "[4/4] Verifying installation..." -ForegroundColor Yellow

$exePath = Join-Path $InstallDir "ALX.CLI.exe"
& $exePath version
if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    if ($IsUpgrade) {
        Write-Host "  ALX upgraded successfully!" -ForegroundColor Green
    } else {
        Write-Host "  ALX installed successfully!" -ForegroundColor Green
    }
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Quick start:" -ForegroundColor Yellow
    Write-Host "  1. Open a NEW terminal window"
    Write-Host "  2. Run: alx version"
    Write-Host '  3. Create hello.alx with: print("Hello!")'
    Write-Host "  4. Run: alx hello.alx"
    Write-Host ""
    Write-Host "VS Code Extension:" -ForegroundColor Yellow
    Write-Host "  code --install-extension $InstallDir\vscode-ext"
    Write-Host ""
    Write-Host "Docs site: https://real-devalex.github.io/alx/" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Host "Installation verification failed." -ForegroundColor Red
    exit 1
}
