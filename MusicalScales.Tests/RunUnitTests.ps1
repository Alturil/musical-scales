<#
.SYNOPSIS
    Runs unit tests for the Musical Scales API project with optional code coverage reporting.

.PARAMETER GenerateCoverage
    Whether to generate and open the HTML coverage report. Defaults to $true.

.PARAMETER OpenReport
    Whether to automatically open the coverage report in the default browser. Defaults to $true.

.EXAMPLE
    .\RunUnitTests.ps1
    Runs tests with coverage report generation and opens the report.

.EXAMPLE
    .\RunUnitTests.ps1 -GenerateCoverage $false
    Runs tests only, without generating coverage report.
#>

param(
    [bool]$GenerateCoverage = $true,
    [bool]$OpenReport = $true
)

function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Main script
Write-Host ""
Write-Host "Musical Scales API - Unit Test Runner" -ForegroundColor Magenta
Write-Host "=====================================" -ForegroundColor Magenta
Write-Host ""

# Check directory
if (!(Test-Path "MusicalScales.Tests.csproj")) {
    Write-Error "This script must be run from the MusicalScales.Tests directory"
    exit 1
}

# Clean up previous results
Write-Status "Cleaning up previous test results..."
if (Test-Path "TestResults") {
    Remove-Item "TestResults" -Recurse -Force
    Write-Success "Previous test results cleaned up"
}

# Run tests
Write-Status "Running unit tests..."
if ($GenerateCoverage) {
    Write-Host "[INFO] Coverage collection enabled" -ForegroundColor Yellow
    dotnet test --collect:"XPlat Code Coverage" --verbosity normal
} else {
    Write-Host "[INFO] Running tests without coverage collection" -ForegroundColor Yellow
    dotnet test --verbosity normal
}

if ($LASTEXITCODE -ne 0) {
    Write-Error "Unit tests failed!"
    exit $LASTEXITCODE
}

Write-Success "All unit tests passed!"

# Generate coverage report
if ($GenerateCoverage) {
    Write-Status "Generating HTML coverage report..."
    
    # Check if ReportGenerator is installed
    try {
        Get-Command "reportgenerator" -ErrorAction Stop | Out-Null
    } catch {
        Write-Status "Installing ReportGenerator..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to install ReportGenerator"
            exit $LASTEXITCODE
        }
        Write-Success "ReportGenerator installed successfully"
    }
    
    # Find coverage files
    $coverageFiles = Get-ChildItem -Path "TestResults" -Filter "coverage.cobertura.xml" -Recurse
    if ($coverageFiles.Count -eq 0) {
        Write-Error "No coverage files found in TestResults directory"
        exit 1
    }
    
    # Generate report
    $reportDir = "TestResults/CoverageReport"
    reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"$reportDir" -reporttypes:Html
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to generate coverage report"
        exit $LASTEXITCODE
    }
    
    Write-Success "Coverage report generated in: $reportDir"
    
    # Open report
    if ($OpenReport) {
        $indexPath = Join-Path $reportDir "index.html"
        if (Test-Path $indexPath) {
            Write-Status "Opening coverage report in default browser..."
            Start-Process $indexPath
            Write-Success "Coverage report opened successfully"
        } else {
            Write-Error "Coverage report index.html not found at: $indexPath"
        }
    }
    
    Write-Host ""
    Write-Host "Coverage Report Summary" -ForegroundColor Magenta
    Write-Host "======================" -ForegroundColor Magenta
    Write-Host "Report Location: $reportDir" -ForegroundColor White
    Write-Host "Open manually: Start-Process '$reportDir\index.html'" -ForegroundColor White
}

Write-Host ""
Write-Host "Test Run Complete!" -ForegroundColor Green
Write-Host "=================" -ForegroundColor Green
Write-Host ""