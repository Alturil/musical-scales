# Script to get all scales and their pitches with a specified root
param(
    [Parameter(Mandatory=$false)]
    [string]$RootNote = "C"
)

$baseUrl = "http://localhost:5000/api"

# Get all scales using Invoke-RestMethod
Write-Host "Fetching all scales..." -ForegroundColor Cyan
$scales = Invoke-RestMethod -Uri "$baseUrl/scales" -Method Get

Write-Host "`nFound $($scales.Count) scales`n" -ForegroundColor Green

# For each scale, get the pitches
$results = @()
foreach ($scale in $scales) {
    $scaleName = $scale.metadata.names[0]

    Write-Host "Getting pitches for: $scaleName" -ForegroundColor Gray

    try {
        # Call the pitches endpoint using Invoke-RestMethod instead of curl
        $pitchesUri = "$baseUrl/scales/$($scale.id)/pitches"
        $rootPitch = @{
            name = $RootNote
            accidental = "Natural"
        }
        $pitches = Invoke-RestMethod -Uri $pitchesUri -Method Post -Body ($rootPitch | ConvertTo-Json) -ContentType "application/json"

        # Format pitches as strings
        $pitchStrings = @()
        foreach ($pitch in $pitches) {
            $accidental = if ($pitch.accidental -eq "Natural") { "" } else {
                switch ($pitch.accidental) {
                    "Sharp" { "#" }
                    "Flat" { "b" }
                    "DoubleSharp" { "##" }
                    "DoubleFlat" { "bb" }
                    default { $pitch.accidental }
                }
            }
            $pitchStrings += "$($pitch.name)$accidental"
        }

        $results += [PSCustomObject]@{
            Scale = $scaleName
            Pitches = $pitchStrings -join " - "
        }
    }
    catch {
        Write-Host "Error getting pitches for $scaleName : $_" -ForegroundColor Red
    }
}

# Display results
Write-Host "`n" -NoNewline
Write-Host "SCALE PITCHES (Root: $RootNote)" -ForegroundColor Yellow
Write-Host ("=" * 80) -ForegroundColor Yellow
$results | Format-Table -Property Scale, @{Label="Pitches"; Expression={$_.Pitches}; Width=70} -Wrap
