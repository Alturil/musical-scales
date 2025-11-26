# Debugging Scripts

This folder contains PowerShell scripts and test data files used for debugging and testing the Musical Scales API during development.

## Scripts

### get-scale-pitches.ps1

Fetches all scales from the API and displays their pitches starting from a specified root note.

**Usage:**
```powershell
.\get-scale-pitches.ps1 [-RootNote <note>]
```

**Parameters:**
- `-RootNote` (optional): The root note to use (default: C). Examples: C, D, E, F, G, A, B

**Examples:**
```powershell
# Get all scales with C as root
.\get-scale-pitches.ps1

# Get all scales with E as root
.\get-scale-pitches.ps1 -RootNote E

# Get all scales with F# as root
.\get-scale-pitches.ps1 -RootNote F
```

**Output:**
Displays a formatted table showing each scale and its pitches.

### fetch-scale-pitches.ps1

Similar to get-scale-pitches.ps1 but with different output formatting (legacy version).

### test-api.ps1

General API testing script for making various requests to the Musical Scales API endpoints.

### test-major-pitches.ps1

Tests the pitch calculation specifically for the Major scale.

### check-intervals.ps1

Checks and validates interval data in scales.

## Test Data Files

### scales-data.json

Sample scale data used for testing.

### scales-response.json

Sample API response data for scales endpoint.

### test-scales.json

Test scale definitions used during development.

## Requirements

- PowerShell 5.1 or later
- Musical Scales API running locally on `http://localhost:5000`
- Database populated with scales (use `Populate-Scales.ps1` from the root directory)

## Notes

These scripts are intended for development and debugging purposes only. For production use cases, refer to the main API documentation and use the official API endpoints directly.
