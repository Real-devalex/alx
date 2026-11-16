# ALX VS Code Extension Installer
# Installs the ALX extension (colours, snippets, completions, hover help, Run button, ALX Night theme).
# Usage: .\install-vscode.ps1

$ErrorActionPreference = 'Stop'
Write-Host ""
Write-Host "Installing the ALX VS Code extension..." -ForegroundColor Cyan

$code = Get-Command code -ErrorAction SilentlyContinue
if (-not $code) {
    Write-Host "  I could not find the 'code' command." -ForegroundColor Red
    Write-Host "  In VS Code press Ctrl+Shift+P and run: Shell Command: Install 'code' command in PATH" -ForegroundColor Yellow
    Write-Host "  Or open the Extensions view, click ... and choose Install from VSIX." -ForegroundColor Yellow
    exit 1
}

# Use a .vsix built here, otherwise download the one from the latest release.
$vsix = Get-ChildItem "$PSScriptRoot\vscode-extension\*.vsix" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($vsix) {
    $path = $vsix.FullName
    Write-Host "  Using $($vsix.Name)" -ForegroundColor Green
} else {
    $path = Join-Path $env:TEMP "alx-language-support.vsix"
    Write-Host "  Downloading the latest extension..." -ForegroundColor Yellow
    Invoke-WebRequest "https://github.com/Real-devalex/alx/releases/latest/download/alx-language-support.vsix" -OutFile $path
}

& code --install-extension $path --force
Write-Host ""
Write-Host "Done. Restart VS Code, open a .alx file and press the play button." -ForegroundColor Green
Write-Host "Tip: Ctrl+Shift+P, 'Preferences: Color Theme', then pick ALX Night." -ForegroundColor Cyan
