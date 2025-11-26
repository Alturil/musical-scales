param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('local', 'dev')]
    [string]$Environment = 'local',

    [Parameter(Mandatory=$false)]
    [string]$ApiKey = ''
)

# Set base URL based on environment
$baseUrl = switch ($Environment) {
    'local' { 'http://localhost:5000' }
    'dev'   { 'https://8ptsg8z6u3.execute-api.ap-southeast-2.amazonaws.com/v1' }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Musical Scales API - Populate Scales" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow
Write-Host ""

# Get the script directory and construct path to JSON files
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$jsonPath = Join-Path $scriptDir "MusicalScales.Api\Data\SeedData\Scales"

# Check if directory exists
if (-not (Test-Path $jsonPath)) {
    Write-Host "ERROR: JSON directory not found: $jsonPath" -ForegroundColor Red
    exit 1
}

# Get all JSON files
$jsonFiles = Get-ChildItem -Path $jsonPath -Filter "*.json" | Sort-Object Name

if ($jsonFiles.Count -eq 0) {
    Write-Host "ERROR: No JSON files found in: $jsonPath" -ForegroundColor Red
    exit 1
}

Write-Host "Found $($jsonFiles.Count) scale files to process`n" -ForegroundColor Green

# Prepare headers
$headers = @{
    "Content-Type" = "application/json"
}

# Add API key header if provided
if ($ApiKey) {
    $headers["x-api-key"] = $ApiKey
}

# Process each JSON file
$successCount = 0
$errorCount = 0

foreach ($file in $jsonFiles) {
    $scaleName = $file.BaseName
    Write-Host "Processing: $scaleName..." -ForegroundColor Gray

    try {
        # Read JSON content
        $jsonContent = Get-Content $file.FullName -Raw

        # Post to API
        $response = Invoke-RestMethod -Uri "$baseUrl/api/scales" -Method Post -Body $jsonContent -Headers $headers -ContentType "application/json"

        Write-Host "  Success" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        $errorCount++
    }
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total files: $($jsonFiles.Count)" -ForegroundColor White
Write-Host "Successful: $successCount" -ForegroundColor Green

if ($errorCount -gt 0) {
    Write-Host "Failed: $errorCount" -ForegroundColor Red
}
else {
    Write-Host "Failed: $errorCount" -ForegroundColor White
}

Write-Host ""

if ($successCount -eq $jsonFiles.Count) {
    Write-Host "All scales populated successfully!" -ForegroundColor Green
    exit 0
}
else {
    Write-Host "Some scales failed to populate" -ForegroundColor Yellow
    exit 1
}
