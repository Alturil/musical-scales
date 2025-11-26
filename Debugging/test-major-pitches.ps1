$scales = Invoke-RestMethod -Uri "http://localhost:5000/api/scales" -Method Get
$major = $scales | Where-Object { $_.metadata.names -contains "Major" }

Write-Host "Major Scale ID: $($major.id)" -ForegroundColor Cyan

$rootPitch = @{
    name = "C"
    accidental = "Natural"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/api/scales/$($major.id)/pitches" -Method Post -Body $rootPitch -ContentType "application/json"

Write-Host "`nMajor Scale Pitches:" -ForegroundColor Cyan
$response | Format-Table name, accidental
