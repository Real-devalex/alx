# ALX Build Script — Windows
# Usage: .\build.ps1

Write-Host "Building ALX..." -ForegroundColor Cyan

# Build the solution
dotnet build ALX.sln -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Run all tests
Write-Host "`nRunning tests..." -ForegroundColor Cyan
dotnet test ALX.sln --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

# Create output directory
$outputDir = "dist"
if (Test-Path $outputDir) { Remove-Item -Recurse -Force $outputDir }
New-Item -ItemType Directory -Path $outputDir | Out-Null

# Publish CLI
Write-Host "`nPublishing CLI..." -ForegroundColor Cyan
dotnet publish src/ALX.CLI -c Release -r win-x64 --self-contained false -o "$outputDir/alx"

# Copy examples
Copy-Item -Recurse examples "$outputDir/alx/examples"

# Copy documentation
Copy-Item -Recurse docs/site "$outputDir/alx/docs"

# Create alx.exe wrapper (copy ALX.CLI.exe as alx.exe)
Copy-Item "$outputDir/alx/ALX.CLI.exe" "$outputDir/alx/alx.exe"

# Create alx.bat launcher for PATH usage
$batContent = @"
@echo off
"%~dp0ALX.CLI.exe" %*
"@
Set-Content -Path "$outputDir/alx/alx.bat" -Value $batContent

Write-Host "`nBuild complete!" -ForegroundColor Green
Write-Host "Output: $outputDir/alx/"
Write-Host ""
Write-Host "To use ALX:" -ForegroundColor Yellow
Write-Host "  Option 1 (recommended):" -ForegroundColor Yellow
Write-Host "    Copy dist\alx\ to C:\alx\ or any folder"
Write-Host "    Add that folder to your system PATH"
Write-Host "    Then run: alx version"
Write-Host ""
Write-Host "  Option 2 (quick test):" -ForegroundColor Yellow
Write-Host "    .\dist\alx\alx.bat version"
Write-Host "    .\dist\alx\alx.bat examples\hello.alx"
