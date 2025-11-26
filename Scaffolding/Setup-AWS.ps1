<#
.SYNOPSIS
    Sets up AWS infrastructure for Musical Scales API deployment.

.DESCRIPTION
    This script creates all necessary AWS resources for deploying the Musical Scales API:
    - S3 bucket for Terraform state
    - GitHub OIDC provider for authentication
    - IAM role for GitHub Actions
    - IAM policies for deployment

    The script is idempotent - it can be run multiple times safely.

.PARAMETER TfStateBucket
    S3 bucket name for Terraform state (must be globally unique). If not provided, will prompt.

.PARAMETER Environment
    Environment name (dev/staging/prod). Defaults to 'dev'.

.EXAMPLE
    .\Setup-AWS.ps1

.EXAMPLE
    .\Setup-AWS.ps1 -TfStateBucket "my-musical-scales-terraform-state"

.EXAMPLE
    .\Setup-AWS.ps1 -TfStateBucket "my-tf-state" -Environment "prod"
#>

[CmdletBinding()]
param(
    [string]$TfStateBucket,
    [string]$Environment = "dev"
)

# Error handling
$ErrorActionPreference = "Stop"

# Colors for output
function Write-Success { param($Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Warn { param($Message) Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-Fail { param($Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }

function Get-AWSRegion {
    Write-Info "Detecting AWS region..."

    # Try to get from AWS CLI config
    try {
        $region = aws configure get region
        if ($region) {
            Write-Success "Detected AWS region from CLI config: $region"
            return $region
        }
    }
    catch {
        # Fall through to prompt
    }

    Write-Warn "AWS region not configured in AWS CLI"
    $region = Read-Host "Enter AWS region"

    if ([string]::IsNullOrWhiteSpace($region)) {
        Write-Fail "AWS region is required"
        exit 1
    }

    Write-Success "Using AWS region: $region"
    return $region
}

function Get-GitHubInfo {
    Write-Info "Detecting GitHub repository info..."

    try {
        # Get git remote URL
        $remoteUrl = git config --get remote.origin.url

        if (-not $remoteUrl) {
            throw "No git remote found"
        }

        # Parse GitHub org and repo from URL
        # Supports both HTTPS and SSH formats:
        # https://github.com/username/repo.git
        # git@github.com:username/repo.git

        if ($remoteUrl -match 'github\.com[:/]([^/]+)/([^/\.]+)') {
            $org = $matches[1]
            $repo = $matches[2]

            Write-Success "Detected GitHub repository: $org/$repo"

            return @{
                Org = $org
                Repo = $repo
            }
        }
        else {
            throw "Could not parse GitHub URL: $remoteUrl"
        }
    }
    catch {
        Write-Fail "Failed to detect GitHub repository: $_"
        Write-Info "Make sure you're in a git repository with a GitHub remote"
        exit 1
    }
}

function Test-S3BucketAvailability {
    param([string]$BucketName)

    Write-Info "Checking bucket availability: $BucketName"

    # Temporarily allow errors since we expect head-bucket to fail for non-existent buckets
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    $result = aws s3api head-bucket --bucket $BucketName 2>&1 | Out-String
    $exitCode = $LASTEXITCODE

    $ErrorActionPreference = $previousErrorAction

    if ($exitCode -eq 0) {
        Write-Success "Bucket exists and you own it"
        return $true
    }
    elseif ($result -like "*404*" -or $result -like "*NoSuchBucket*") {
        Write-Success "Bucket name is available"
        return $true
    }
    elseif ($result -like "*403*" -or $result -like "*Forbidden*") {
        Write-Fail "Bucket name $BucketName is already taken by another AWS account"
        Write-Info "Please choose a different bucket name and try again"
        exit 1
    }
    else {
        Write-Fail "Could not check bucket availability. Exit code: $exitCode"
        Write-Info "Error output: $result"
        exit 1
    }
}

function Get-TerraformStateBucket {
    param([string]$Bucket)

    $defaultBucket = "musical-scales-terraform-state"

    if (-not [string]::IsNullOrWhiteSpace($Bucket)) {
        $validPattern = '^[a-z0-9][a-z0-9-]*[a-z0-9]$'
        $isValidLength = ($Bucket.Length -ge 3) -and ($Bucket.Length -le 63)
        $isValidFormat = $Bucket -match $validPattern

        if (-not $isValidLength -or -not $isValidFormat) {
            Write-Fail "Invalid bucket name: $Bucket"
            Write-Info "Must be 3-63 characters, lowercase, numbers, and hyphens only"
            exit 1
        }
        Test-S3BucketAvailability -BucketName $Bucket
        return $Bucket
    }

    Write-Info "Trying default bucket name: $defaultBucket"

    # Temporarily allow errors since we expect head-bucket to fail for non-existent buckets
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    $result = aws s3api head-bucket --bucket $defaultBucket 2>&1 | Out-String
    $exitCode = $LASTEXITCODE

    $ErrorActionPreference = $previousErrorAction

    if ($exitCode -eq 0) {
        Write-Success "Using existing bucket: $defaultBucket"
        return $defaultBucket
    }
    elseif ($result -like "*404*" -or $result -like "*NoSuchBucket*") {
        Write-Success "Bucket name is available: $defaultBucket"
        return $defaultBucket
    }
    elseif ($result -like "*403*" -or $result -like "*Forbidden*") {
        Write-Warn "Default bucket name $defaultBucket is already taken by another AWS account"
        Write-Host ""
        Write-Host "Please provide an alternative bucket name" -ForegroundColor Cyan
        Write-Host ""

        $bucket = Read-Host "Enter S3 bucket name for Terraform state"

        if ([string]::IsNullOrWhiteSpace($bucket)) {
            Write-Fail "S3 bucket name is required"
            exit 1
        }

        $validPattern = '^[a-z0-9][a-z0-9-]*[a-z0-9]$'
        $isValidLength = ($bucket.Length -ge 3) -and ($bucket.Length -le 63)
        $isValidFormat = $bucket -match $validPattern

        if (-not $isValidLength -or -not $isValidFormat) {
            Write-Fail "Invalid bucket name format"
            Write-Info "Must be 3-63 characters, lowercase letters, numbers, and hyphens only"
            exit 1
        }

        Test-S3BucketAvailability -BucketName $bucket
        return $bucket
    }
    else {
        Write-Fail "Could not check bucket availability. Exit code: $exitCode"
        Write-Info "Error output: $result"
        exit 1
    }
}

function Test-AWSCli {
    Write-Info "Checking AWS CLI installation..."

    try {
        $version = aws --version 2>&1
        Write-Success "AWS CLI is installed: $version"
    }
    catch {
        Write-Fail "AWS CLI is not installed or not in PATH"
        Write-Info "Install with: choco install awscli"
        exit 1
    }
}

function Test-AWSCredentials {
    Write-Info "Checking AWS credentials..."

    try {
        $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
        Write-Success "AWS credentials are configured"
        Write-Info "Account: $($identity.Account)"
        Write-Info "User: $($identity.Arn)"
        return $identity.Account
    }
    catch {
        Write-Fail "AWS credentials are not configured"
        Write-Info "Run: aws login"
        Write-Info "Select ap-southeast-2 as your region"
        exit 1
    }
}

function Create-S3Bucket {
    param(
        [string]$BucketName,
        [string]$Region
    )

    Write-Info "Checking S3 bucket: $BucketName"

    # Check if bucket exists
    $bucketExists = $false
    try {
        $null = aws s3api head-bucket --bucket $BucketName 2>&1
        $bucketExists = $true
        Write-Success "S3 bucket already exists: $BucketName"
    }
    catch {
        Write-Info "S3 bucket does not exist, creating..."
    }

    if (-not $bucketExists) {
        try {
            # Create bucket (us-east-1 has different syntax)
            if ($Region -eq "us-east-1") {
                aws s3api create-bucket --bucket $BucketName --region $Region
            }
            else {
                aws s3api create-bucket --bucket $BucketName --region $Region --create-bucket-configuration LocationConstraint=$Region
            }
            Write-Success "Created S3 bucket: $BucketName"

            # Enable versioning
            aws s3api put-bucket-versioning --bucket $BucketName --versioning-configuration Status=Enabled
            Write-Success "Enabled versioning on S3 bucket"

            # Enable encryption
            Push-Location $PSScriptRoot
            aws s3api put-bucket-encryption --bucket $BucketName --server-side-encryption-configuration file://s3-encryption-config.json
            Pop-Location
            Write-Success "Enabled encryption on S3 bucket"

            # Block public access
            aws s3api put-public-access-block --bucket $BucketName --public-access-block-configuration BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true
            Write-Success "Enabled public access block on S3 bucket"
        }
        catch {
            Write-Fail "Failed to create S3 bucket: $_"
            if ($_.Exception.Message -like "*BucketAlreadyExists*") {
                Write-Info "This bucket name is already taken by another AWS account. Please choose a different name."
            }
            exit 1
        }
    }

    # Verify versioning is enabled
    $versioning = aws s3api get-bucket-versioning --bucket $BucketName --output json | ConvertFrom-Json
    if ($versioning.Status -ne "Enabled") {
        Write-Warn "Versioning is not enabled, enabling now..."
        aws s3api put-bucket-versioning --bucket $BucketName --versioning-configuration Status=Enabled
        Write-Success "Enabled versioning"
    }

    # Verify encryption is enabled
    $encryptionEnabled = $false
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    try {
        $encryptionResult = aws s3api get-bucket-encryption --bucket $BucketName --output json 2>&1
        if ($LASTEXITCODE -eq 0) {
            $encryption = $encryptionResult | ConvertFrom-Json
            if ($encryption.ServerSideEncryptionConfiguration.Rules) {
                $encryptionEnabled = $true
            }
        }
    }
    catch {
        # Encryption not configured
    }

    $ErrorActionPreference = $previousErrorAction

    if (-not $encryptionEnabled) {
        Write-Warn "Encryption is not enabled, enabling now..."

        # Use file:// to avoid JSON corruption issues
        Push-Location $PSScriptRoot
        $encResult = aws s3api put-bucket-encryption --bucket $BucketName --server-side-encryption-configuration file://s3-encryption-config.json 2>&1
        $encExitCode = $LASTEXITCODE
        Pop-Location

        if ($encExitCode -eq 0) {
            Write-Success "Enabled encryption"
        }
        else {
            Write-Fail "Failed to enable encryption: $encResult"
            exit 1
        }
    }
}

function Create-OIDCProvider {
    param([string]$AccountId)

    Write-Info "Checking GitHub OIDC provider..."

    # Check if OIDC provider exists
    $oidcArn = "arn:aws:iam::${AccountId}:oidc-provider/token.actions.githubusercontent.com"

    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    $checkOidcResult = aws iam get-open-id-connect-provider --open-id-connect-provider-arn $oidcArn 2>&1 | Out-String
    $checkOidcExitCode = $LASTEXITCODE

    $ErrorActionPreference = $previousErrorAction

    if ($checkOidcExitCode -eq 0) {
        Write-Success "GitHub OIDC provider already exists"
        return $oidcArn
    }

    Write-Info "Creating GitHub OIDC provider..."

    $ErrorActionPreference = "Continue"

    $createOidcResult = aws iam create-open-id-connect-provider `
        --url https://token.actions.githubusercontent.com `
        --client-id-list sts.amazonaws.com `
        --thumbprint-list 6938fd4d98bab03faadb97b34396831e3780aea1 2>&1 | Out-String

    $createOidcExitCode = $LASTEXITCODE
    $ErrorActionPreference = $previousErrorAction

    if ($createOidcExitCode -ne 0) {
        Write-Fail "Failed to create OIDC provider"
        Write-Info "Error: $createOidcResult"
        exit 1
    }

    Write-Success "Created GitHub OIDC provider"
    return $oidcArn
}

function Create-IAMRole {
    param(
        [string]$AccountId,
        [string]$GitHubOrg,
        [string]$GitHubRepo
    )

    $roleName = "GitHubActionsMusicalScales"
    Write-Info "Checking IAM role: $roleName"

    # Check if role exists
    $roleExists = $false
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    $checkRoleResult = aws iam get-role --role-name $roleName --output json 2>&1 | Out-String
    $checkRoleExitCode = $LASTEXITCODE

    $ErrorActionPreference = $previousErrorAction

    if ($checkRoleExitCode -eq 0) {
        $roleExists = $true
        Write-Success "IAM role already exists: $roleName"
    }
    else {
        Write-Info "Creating IAM role..."
    }

    if (-not $roleExists) {
        # Load and customize trust policy template
        $trustPolicyTemplate = Get-Content "$PSScriptRoot\trust-policy-template.json" -Raw
        $trustPolicy = $trustPolicyTemplate -replace 'ACCOUNT_ID', $AccountId `
                                             -replace 'GITHUB_ORG', $GitHubOrg `
                                             -replace 'GITHUB_REPO', $GitHubRepo

        $previousErrorAction = $ErrorActionPreference
        $ErrorActionPreference = "Continue"

        # Pass policy via CLI argument instead of file to avoid path issues
        $createRoleResult = aws iam create-role `
            --role-name $roleName `
            --assume-role-policy-document $trustPolicy `
            --description "Role for GitHub Actions to deploy Musical Scales API" 2>&1 | Out-String

        $createRoleExitCode = $LASTEXITCODE
        $ErrorActionPreference = $previousErrorAction

        if ($createRoleExitCode -ne 0) {
            Write-Fail "Failed to create IAM role"
            Write-Info "Error: $createRoleResult"
            exit 1
        }

        Write-Success "Created IAM role: $roleName"
    }

    return $roleName
}

function Attach-IAMPolicy {
    param(
        [string]$RoleName,
        [string]$BucketName
    )

    $policyName = "GitHubActionsMusicalScalesPolicy"
    Write-Info "Checking IAM policy: $policyName"

    # Check if policy is already attached
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    $checkResult = aws iam get-role-policy --role-name $($RoleName.Trim()) --policy-name $policyName 2>&1 | Out-String
    $checkExitCode = $LASTEXITCODE

    $ErrorActionPreference = $previousErrorAction

    # Load and customize role policy template
    $policyTemplate = Get-Content "$PSScriptRoot\role-policy-template.json" -Raw
    $policy = $policyTemplate -replace 'TF_STATE_BUCKET', $BucketName

    if ($checkExitCode -eq 0) {
        Write-Info "IAM policy already exists, updating..."
    }
    else {
        Write-Info "Attaching IAM policy..."
    }

    # Validate JSON before applying
    try {
        $null = $policy | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        Write-Fail "Generated policy has invalid JSON syntax"
        Write-Info "Policy content:"
        Write-Host $policy
        Write-Info "JSON Error: $_"
        exit 1
    }

    # Write policy document to file with UTF-8 encoding without BOM
    $policyDocFile = "$PSScriptRoot\policy-document.json"
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($policyDocFile, $policy, $utf8NoBom)

    # Temporarily allow errors to check result
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    # Change to script directory and use relative path for file://
    Push-Location $PSScriptRoot
    $result = aws iam put-role-policy `
        --role-name $($RoleName.Trim()) `
        --policy-name $policyName `
        --policy-document "file://policy-document.json" 2>&1 | Out-String
    Pop-Location

    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = $previousErrorAction

    # Clean up temp file
    Remove-Item $policyDocFile -ErrorAction SilentlyContinue

    if ($exitCode -ne 0) {
        Write-Fail "Failed to update IAM policy"
        Write-Info "Error: $result"
        Write-Info "Policy document that was used:"
        Write-Host $policy
        exit 1
    }

    if ($checkExitCode -eq 0) {
        Write-Success "Updated IAM policy: $policyName"
    }
    else {
        Write-Success "Attached IAM policy: $policyName"
    }
}

function Show-GitHubSecrets {
    param(
        [string]$RoleName,
        [string]$Region,
        [string]$BucketName,
        [string]$Environment
    )

    Write-Host ""
    Write-Host "===============================================================" -ForegroundColor Cyan
    Write-Host "  GitHub Secrets Configuration" -ForegroundColor Cyan
    Write-Host "===============================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Add these secrets to your GitHub repository:" -ForegroundColor Yellow
    Write-Host "Settings -> Secrets and variables -> Actions -> New repository secret" -ForegroundColor Gray
    Write-Host ""

    # Get role ARN with proper error handling
    $previousErrorAction = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    $roleArn = aws iam get-role --role-name $($RoleName.Trim()) --query 'Role.Arn' --output text 2>&1
    $exitCode = $LASTEXITCODE

    $ErrorActionPreference = $previousErrorAction

    if ($exitCode -ne 0) {
        Write-Warn "Could not retrieve role ARN"
        $roleArn = "arn:aws:iam::ACCOUNT_ID:role/$RoleName"
    }

    Write-Host "1. AWS_REGION" -ForegroundColor Green
    Write-Host "   $Region" -ForegroundColor White
    Write-Host ""

    Write-Host "2. AWS_ROLE_ARN" -ForegroundColor Green
    Write-Host "   $roleArn" -ForegroundColor White
    Write-Host ""

    Write-Host "3. TF_STATE_BUCKET" -ForegroundColor Green
    Write-Host "   $BucketName" -ForegroundColor White
    Write-Host ""

    Write-Host "4. ENVIRONMENT" -ForegroundColor Green
    Write-Host "   $Environment" -ForegroundColor White
    Write-Host ""

    Write-Host "===============================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "After adding these secrets, you can push to main and deploy!"
    Write-Host ""
}

# Main execution
try {
    Write-Host ""
    Write-Host "===============================================================" -ForegroundColor Magenta
    Write-Host "  Musical Scales API - AWS Setup" -ForegroundColor Magenta
    Write-Host "===============================================================" -ForegroundColor Magenta
    Write-Host ""

    # Step 1: Verify prerequisites
    Test-AWSCli
    $accountId = Test-AWSCredentials
    Write-Host ""

    # Step 2: Auto-detect configuration
    $region = Get-AWSRegion
    $githubInfo = Get-GitHubInfo
    $bucketName = Get-TerraformStateBucket -Bucket $TfStateBucket

    Write-Host ""
    Write-Host "---------------------------------------------------------------" -ForegroundColor Gray
    Write-Info "Configuration Summary:"
    Write-Host "  AWS Region: $region" -ForegroundColor Gray
    Write-Host "  GitHub: $($githubInfo.Org)/$($githubInfo.Repo)" -ForegroundColor Gray
    Write-Host "  S3 Bucket: $bucketName" -ForegroundColor Gray
    Write-Host "  Environment: $Environment" -ForegroundColor Gray
    Write-Host "---------------------------------------------------------------" -ForegroundColor Gray
    Write-Host ""

    # Confirm before proceeding
    $confirm = Read-Host "Proceed with AWS resource creation? (Y/n)"
    if ($confirm -and $confirm -ne "Y" -and $confirm -ne "y" -and $confirm -ne "") {
        Write-Warn "Setup cancelled by user"
        exit 0
    }

    Write-Host ""
    Write-Host "---------------------------------------------------------------" -ForegroundColor Gray
    Write-Info "Starting AWS resource creation..."
    Write-Host "---------------------------------------------------------------" -ForegroundColor Gray
    Write-Host ""

    # Step 3: Create resources
    Create-S3Bucket -BucketName $bucketName -Region $region
    Write-Host ""

    Create-OIDCProvider -AccountId $accountId
    Write-Host ""

    $roleName = Create-IAMRole -AccountId $accountId -GitHubOrg $githubInfo.Org -GitHubRepo $githubInfo.Repo
    Write-Host ""

    Attach-IAMPolicy -RoleName $roleName -BucketName $bucketName
    Write-Host ""

    # Step 4: Show results
    Write-Host "---------------------------------------------------------------" -ForegroundColor Gray
    Write-Success "AWS setup completed successfully!"
    Write-Host "---------------------------------------------------------------" -ForegroundColor Gray

    Show-GitHubSecrets -RoleName $roleName -Region $region -BucketName $bucketName -Environment $Environment
}
catch {
    Write-Host ""
    Write-Fail "Setup failed: $_"
    Write-Host ""
    exit 1
}
