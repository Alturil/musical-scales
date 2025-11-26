$response = Invoke-WebRequest -Uri http://localhost:5000/api/scales -UseBasicParsing
Write-Host "Status: $($response.StatusCode)"
Write-Host "Content length: $($response.Content.Length)"
$response.Content | Out-File -FilePath scales-data.json -Encoding UTF8
Write-Host "Saved to scales-data.json"
