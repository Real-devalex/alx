# ALX VS Code Extension Installer
# Run this script from the ALX project root to install the extension
# Usage: .\install-vscode.ps1

$ExtensionsDir = "$env:USERPROFILE\.vscode\extensions"
$ExtensionDir = "$ExtensionsDir\alx-language-support-0.4.0"

Write-Host ""
Write-Host "Installing ALX VS Code Extension..." -ForegroundColor Cyan
Write-Host ""

# Remove old version if exists
if (Test-Path $ExtensionDir) {
    Remove-Item -Recurse -Force $ExtensionDir
    Write-Host "  Removed old version" -ForegroundColor Yellow
}

# Create directories
New-Item -ItemType Directory -Path "$ExtensionDir\syntaxes" -Force | Out-Null
New-Item -ItemType Directory -Path "$ExtensionDir\icons" -Force | Out-Null

# Copy extension files
Copy-Item "vscode-extension\package.json" "$ExtensionDir\"
Copy-Item "vscode-extension\extension.js" "$ExtensionDir\"
Copy-Item "vscode-extension\language-configuration.json" "$ExtensionDir\"
Copy-Item "vscode-extension\syntaxes\ALX.tmLanguage.json" "$ExtensionDir\syntaxes\"
Copy-Item "vscode-extension\icon.svg" "$ExtensionDir\"
Copy-Item "vscode-extension\icons\*.svg" "$ExtensionDir\icons\"

Write-Host "  Extension installed to:" -ForegroundColor Green
Write-Host "  $ExtensionDir" -ForegroundColor Gray
Write-Host ""
Write-Host "Restart VS Code to activate the extension." -ForegroundColor Yellow
Write-Host ""
Write-Host "After restart:" -ForegroundColor Cyan
Write-Host "  - Open any .alx file"
Write-Host "  - Click the play button (top-right) or press Ctrl+Alt+R to run"
Write-Host "  - Syntax highlighting and file icons work automatically"
Write-Host ""
