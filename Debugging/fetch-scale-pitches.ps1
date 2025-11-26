# Fetch all scales and their pitches from the API
$baseUrl = "http://localhost:5000/api"

# Create a web client
$webClient = New-Object System.Net.WebClient
$webClient.Headers.Add("Content-Type", "application/json")

Write-Host "Fetching scales from $baseUrl/scales..." -ForegroundColor Cyan

try {
    # Get all scales
    $scalesJson = $webClient.DownloadString("$baseUrl/scales")
    $scales = $scalesJson | ConvertFrom-Json

    Write-Host "Found $($scales.Count) scales`n" -ForegroundColor Green

    # Root pitch (C Natural)
    $rootPitch = @{
        name = "C"
        accidental = "Natural"
    } | ConvertTo-Json

    # Results array
    $results = @()

    foreach ($scale in $scales) {
        $scaleName = $scale.metadata.names[0]
        Write-Host "Processing: $scaleName..." -ForegroundColor Gray

        try {
            # Get pitches for this scale
            $webClient.Headers["Content-Type"] = "application/json"
            $pitchesJson = $webClient.UploadString("$baseUrl/scales/$($scale.id)/pitches", "POST", $rootPitch)
            $pitches = $pitchesJson | ConvertFrom-Json

            # Format pitch names
            $pitchNames = @()
            foreach ($pitch in $pitches) {
                $name = $pitch.name
                $acc = switch ($pitch.accidental) {
                    "Natural" { "" }
                    "Sharp" { "#" }
                    "Flat" { "b" }
                    "DoubleSharp" { "x" }
                    "DoubleFlat" { "bb" }
                    default { "" }
                }
                $pitchNames += "$name$acc"
            }

            $results += [PSCustomObject]@{
                Scale = $scaleName
                Pitches = ($pitchNames -join ", ")
            }
        }
        catch {
            Write-Host "  ERROR: $_" -ForegroundColor Red
        }
    }

    # Display results
    Write-Host "`n=====================================================================" -ForegroundColor Yellow
    Write-Host "SCALE PITCHES (Root Note: C)" -ForegroundColor Yellow
    Write-Host "=====================================================================" -ForegroundColor Yellow

    $results | Format-Table -Property Scale,Pitches -Wrap

}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
finally {
    $webClient.Dispose()
}
