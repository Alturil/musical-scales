#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Runs integration tests with optional code coverage collection for the Musical Scales API project.

.DESCRIPTION
    This script executes all integration tests in the MusicalScales.IntegrationTests project.
    It can optionally collect code coverage information and generate HTML reports.

.PARAMETER GenerateCoverage
    Whether to collect code coverage during test execution. Default: $false

.PARAMETER OpenReport
    Whether to automatically open the coverage report in the default browser. 
    Only applies when GenerateCoverage is $true. Default: $true

.EXAMPLE
    .\RunIntegrationTests.ps1
    Runs integration tests without coverage collection.

.EXAMPLE
    .\RunIntegrationTests.ps1 -GenerateCoverage $true
    Runs integration tests with coverage collection and opens the HTML report.

.EXAMPLE
    .\RunIntegrationTests.ps1 -GenerateCoverage $true -OpenReport $false
    Runs integration tests with coverage collection but doesn't open the report.

.NOTES
    - Requires .NET 8.0 SDK
    - For coverage generation, requires ReportGenerator global tool (auto-installed if missing)
    - Coverage reports are generated in TestResults/CoverageReport directory
    - HTML reports include detailed line-by-line coverage information
#>

param(
    [Parameter(Mandatory = $false)]
    [bool]$GenerateCoverage = $false,
    
    [Parameter(Mandatory = $false)]
    [bool]$OpenReport = $true
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Get the script directory and project paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir
$TestProjectPath = Join-Path $ScriptDir "MusicalScales.IntegrationTests.csproj"
$ApiProjectPath = Join-Path (Join-Path $ProjectDir "MusicalScales.Api") "MusicalScales.Api.csproj"

# Debug: Show calculated paths
Write-Host "Script Directory: $ScriptDir" -ForegroundColor Yellow
Write-Host "Test Project Path: $TestProjectPath" -ForegroundColor Yellow
Write-Host "API Project Path: $ApiProjectPath" -ForegroundColor Yellow

Write-Host "Musical Scales Integration Tests Runner" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Verify required files exist
if (-not (Test-Path $TestProjectPath)) {
    Write-Error "Integration test project not found at: $TestProjectPath"
}

if (-not (Test-Path $ApiProjectPath)) {
    Write-Error "API project not found at: $ApiProjectPath"
}

# Clean any previous test results
$TestResultsDir = Join-Path $ScriptDir "TestResults"
if (Test-Path $TestResultsDir) {
    Write-Host "Cleaning previous test results..." -ForegroundColor Yellow
    Remove-Item $TestResultsDir -Recurse -Force
}

try {
    if ($GenerateCoverage) {
        Write-Host "Running integration tests with code coverage..." -ForegroundColor Green
        
        # Check if ReportGenerator is installed, install if needed
        $ReportGeneratorCheck = dotnet tool list -g | Select-String "reportgenerator"
        if (-not $ReportGeneratorCheck) {
            Write-Host "Installing ReportGenerator global tool..." -ForegroundColor Yellow
            dotnet tool install -g dotnet-reportgenerator-globaltool
        }
        
        # Run tests with coverage collection
        dotnet test $TestProjectPath `
            --configuration Release `
            --collect:"XPlat Code Coverage" `
            --results-directory $TestResultsDir `
            --logger "console;verbosity=normal"
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Integration tests failed with exit code $LASTEXITCODE"
        }
        
        # Find coverage file
        $CoverageFiles = Get-ChildItem -Path $TestResultsDir -Recurse -Filter "coverage.cobertura.xml"
        if ($CoverageFiles.Count -eq 0) {
            Write-Error "No coverage files found in $TestResultsDir"
        }
        
        # Generate HTML coverage report
        Write-Host "Generating coverage report..." -ForegroundColor Green
        $CoverageReportDir = Join-Path $TestResultsDir "CoverageReport"
        
        $CoverageFilePaths = $CoverageFiles | ForEach-Object { $_.FullName }
        $CoveragePathsString = $CoverageFilePaths -join ";"
        
        reportgenerator `
            -reports:$CoveragePathsString `
            -targetdir:$CoverageReportDir `
            -reporttypes:"Html;Cobertura" `
            -verbosity:Info
            
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to generate coverage report with exit code $LASTEXITCODE"
        }
        
        # Display coverage summary
        $IndexPath = Join-Path $CoverageReportDir "index.html"
        if (Test-Path $IndexPath) {
            Write-Host ""
            Write-Host "Coverage report generated successfully!" -ForegroundColor Green
            Write-Host "Report location: $IndexPath" -ForegroundColor Cyan
            
            # Try to extract coverage percentage from the HTML file
            try {
                $htmlContent = Get-Content $IndexPath -Raw
                if ($htmlContent -match 'Line coverage.*?(\d+\.?\d*)%') {
                    $coveragePercentage = $matches[1]
                    Write-Host "Line Coverage: $coveragePercentage%" -ForegroundColor Cyan
                }
            }
            catch {
                # Silently continue if we can't extract coverage percentage
            }
            
            # Open report in default browser if requested
            if ($OpenReport) {
                Write-Host "Opening coverage report in default browser..." -ForegroundColor Green
                if ($IsWindows -or $env:OS -eq "Windows_NT") {
                    Start-Process $IndexPath
                } elseif ($IsMacOS) {
                    & open $IndexPath
                } else {
                    & xdg-open $IndexPath
                }
            }
        } else {
            Write-Warning "Coverage report HTML file not found at expected location: $IndexPath"
        }
    } else {
        Write-Host "Running integration tests..." -ForegroundColor Green
        
        # Run tests without coverage
        dotnet test $TestProjectPath `
            --configuration Release `
            --logger "console;verbosity=normal"
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Integration tests failed with exit code $LASTEXITCODE"
        }
    }
    
    Write-Host ""
    Write-Host "Integration tests completed successfully!" -ForegroundColor Green
    
} catch {
    Write-Host ""
    Write-Host "Error occurred during test execution:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Integration Test Summary:" -ForegroundColor Cyan
Write-Host "- Test Project: MusicalScales.IntegrationTests" -ForegroundColor White
Write-Host "- Coverage Collection: $(if ($GenerateCoverage) { 'Enabled' } else { 'Disabled' })" -ForegroundColor White
if ($GenerateCoverage) {
    Write-Host "- Report Directory: $TestResultsDir" -ForegroundColor White
}
Write-Host ""