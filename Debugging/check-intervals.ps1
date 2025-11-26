$response = Invoke-RestMethod -Uri "http://localhost:5000/api/scales" -Method Get
$major = $response | Where-Object { $_.metadata.names -contains "Major" }

Write-Host "Major Scale Intervals:" -ForegroundColor Cyan
$major.intervals | Format-Table name, quality, pitchOffset, semitoneOffset
