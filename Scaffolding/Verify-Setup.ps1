<#
.SYNOPSIS
    Verifies AWS infrastructure setup for Musical Scales API.

.DESCRIPTION
    This script checks that all AWS resources are properly configured:
    - S3 bucket for Terraform state exists and is configured
    - GitHub OIDC provider exists
    - IAM role exists with correct trust policy
    - IAM policies are attached

    The script auto-detects configuration from AWS CLI and git.

.PARAMETER TfStateBucket
    S3 bucket name for Terraform state. If not provided, will prompt.

.EXAMPLE
    .\Verify-Setup.ps1

.EXAMPLE
    .\Verify-Setup.ps1 -TfStateBucket "my-terraform-state-bucket"
#>

[CmdletBinding()]
param(
    [string]$TfStateBucket
)

$ErrorActionPreference = "Stop"

function Write-Pass { param($Message) Write-Host "[PASS] $Message" -ForegroundColor Green }
function Write-Fail { param($Message) Write-Host "[FAIL] $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "[INFO] $Message" -ForegroundColor Cyan }
function Write-Detail { param($Message) Write-Host "       $Message" -ForegroundColor Gray }

$script:PassCount = 0
$script:FailCount = 0
$script:CurrentCheck = 0
$script:TotalChecks = 10

function Test-Check {
    param(
        [string]$Name,
        [scriptblock]$Check
    )

    $script:CurrentCheck++
    $progress = "[$script:CurrentCheck/$script:TotalChecks]"

    try {
        $result = & $Check
        if ($result) {
            Write-Pass "$progress $Name"
            $script:PassCount++
        }
        else {
            Write-Fail "$progress $Name"
            $script:FailCount++
        }
        return $result
    }
    catch {
        Write-Fail "$progress $Name - $_"
        $script:FailCount++
        return $false
    }
}

function Get-GitHubInfo {
    try {
        $remoteUrl = git config --get remote.origin.url
        if ($remoteUrl -match 'github\.com[:/]([^/]+)/([^/\.]+)') {
            return @{
                Org = $matches[1]
                Repo = $matches[2]
            }
        }
    }
    catch {
        return $null
    }
    return $null
}

function Get-TfStateBucketName {
    param([string]$Bucket)

    if (-not [string]::IsNullOrWhiteSpace($Bucket)) {
        return $Bucket
    }

    # Use default bucket name
    $defaultBucket = "musical-scales-terraform-state"
    Write-Info "Using default bucket name: $defaultBucket"
    return $defaultBucket
}

try {
    Write-Host ""
    Write-Host "===============================================================" -ForegroundColor Magenta
    Write-Host "  Musical Scales API - Setup Verification" -ForegroundColor Magenta
    Write-Host "===============================================================" -ForegroundColor Magenta
    Write-Host ""

    # Get configuration
    $githubInfo = Get-GitHubInfo
    $bucketName = Get-TfStateBucketName -Bucket $TfStateBucket

    # Store in environment variables for tests
    $env:TF_STATE_BUCKET = $bucketName
    if ($githubInfo) {
        $env:GITHUB_ORG = $githubInfo.Org
        $env:GITHUB_REPO = $githubInfo.Repo
    }

    Write-Host ""

    # Check AWS CLI
    $null = Test-Check "AWS CLI installed" {
        try {
            $null = aws --version
            return $true
        }
        catch {
            return $false
        }
    }

    # Check AWS credentials
    $accountId = $null
    $null = Test-Check "AWS credentials configured" {
        try {
            $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
            $script:accountId = $identity.Account
            Write-Detail "Account: $($identity.Account)"
            Write-Detail "User: $($identity.Arn)"
            return $true
        }
        catch {
            return $false
        }
    }

    Write-Host ""

    # Check S3 bucket
    $null = Test-Check "S3 bucket exists: $env:TF_STATE_BUCKET" {
        try {
            $null = aws s3api head-bucket --bucket $env:TF_STATE_BUCKET 2>&1
            return $true
        }
        catch {
            return $false
        }
    }

    $null = Test-Check "S3 bucket versioning enabled" {
        try {
            $versioning = aws s3api get-bucket-versioning --bucket $env:TF_STATE_BUCKET --output json | ConvertFrom-Json
            if ($versioning.Status -eq "Enabled") {
                Write-Detail "Status: Enabled"
                return $true
            }
            Write-Detail "Status: $($versioning.Status)"
            return $false
        }
        catch {
            return $false
        }
    }

    $null = Test-Check "S3 bucket encryption enabled" {
        try {
            $previousErrorAction = $ErrorActionPreference
            $ErrorActionPreference = "Continue"

            $encryptionResult = aws s3api get-bucket-encryption --bucket $env:TF_STATE_BUCKET --output json 2>&1
            $exitCode = $LASTEXITCODE

            $ErrorActionPreference = $previousErrorAction

            if ($exitCode -eq 0) {
                $encryption = $encryptionResult | ConvertFrom-Json
                if ($encryption.ServerSideEncryptionConfiguration.Rules) {
                    Write-Detail "Algorithm: $($encryption.ServerSideEncryptionConfiguration.Rules[0].ApplyServerSideEncryptionByDefault.SSEAlgorithm)"
                    return $true
                }
            }
            return $false
        }
        catch {
            return $false
        }
    }

    Write-Host ""

    # Check OIDC provider
    $null = Test-Check "GitHub OIDC provider exists" {
        try {
            $oidcArn = "arn:aws:iam::${accountId}:oidc-provider/token.actions.githubusercontent.com"
            $null = aws iam get-open-id-connect-provider --open-id-connect-provider-arn $oidcArn 2>&1
            Write-Detail "ARN: $oidcArn"
            return $true
        }
        catch {
            return $false
        }
    }

    Write-Host ""

    # Check IAM role
    $roleName = "GitHubActionsMusicalScales"
    $null = Test-Check "IAM role exists: $roleName" {
        try {
            $role = aws iam get-role --role-name $roleName --output json 2>&1 | ConvertFrom-Json
            Write-Detail "ARN: $($role.Role.Arn)"
            return $true
        }
        catch {
            return $false
        }
    }

    $null = Test-Check "IAM role trust policy configured correctly" {
        try {
            $role = aws iam get-role --role-name $roleName --output json | ConvertFrom-Json
            $trustPolicy = $role.Role.AssumeRolePolicyDocument

            # Check for GitHub OIDC in trust policy
            $hasTrust = $false
            foreach ($statement in $trustPolicy.Statement) {
                if ($statement.Principal.Federated -like "*token.actions.githubusercontent.com" -and
                    $statement.Condition.StringLike.'token.actions.githubusercontent.com:sub' -like "*$env:GITHUB_ORG/$env:GITHUB_REPO*") {
                    $hasTrust = $true
                    Write-Detail "Repository: $env:GITHUB_ORG/$env:GITHUB_REPO"
                    break
                }
            }

            return $hasTrust
        }
        catch {
            return $false
        }
    }

    $null = Test-Check "IAM policy attached: GitHubActionsMusicalScalesPolicy" {
        try {
            $null = aws iam get-role-policy --role-name $roleName --policy-name "GitHubActionsMusicalScalesPolicy" 2>&1
            return $true
        }
        catch {
            return $false
        }
    }

    $null = Test-Check "IAM policy allows S3 access to state bucket" {
        try {
            $policy = aws iam get-role-policy --role-name $roleName --policy-name "GitHubActionsMusicalScalesPolicy" --output json | ConvertFrom-Json
            $policyDoc = $policy.PolicyDocument

            foreach ($statement in $policyDoc.Statement) {
                foreach ($resource in $statement.Resource) {
                    if ($resource -like "*$env:TF_STATE_BUCKET*") {
                        Write-Detail "Bucket: $env:TF_STATE_BUCKET"
                        return $true
                    }
                }
            }

            return $false
        }
        catch {
            return $false
        }
    }

    Write-Host ""
    Write-Host "---------------------------------------------------------------" -ForegroundColor Gray

    # Summary
    Write-Host ""
    if ($script:FailCount -eq 0) {
        Write-Host "All checks passed! ($script:PassCount/$($script:PassCount + $script:FailCount))" -ForegroundColor Green
        Write-Host ""
        Write-Info "Your AWS infrastructure is ready for deployment!"
        Write-Host ""
        Write-Info "Next steps:"
        Write-Host "  1. Configure GitHub secrets (run Setup-AWS.ps1 to see values)" -ForegroundColor Gray
        Write-Host "  2. Push your changes" -ForegroundColor Gray
        Write-Host "  3. Create PR and merge to main" -ForegroundColor Gray
        Write-Host "  4. GitHub Actions will deploy automatically" -ForegroundColor Gray
        Write-Host ""
    }
    else {
        Write-Host "Some checks failed: $script:PassCount passed, $script:FailCount failed" -ForegroundColor Yellow
        Write-Host ""
        Write-Info "To fix issues, run: .\Setup-AWS.ps1"
        Write-Host ""
    }
}
catch {
    Write-Host ""
    Write-Fail "Verification failed: $_"
    Write-Host ""
    exit 1
}
