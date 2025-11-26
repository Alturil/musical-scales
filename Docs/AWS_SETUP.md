# AWS Setup Guide for Musical Scales API

This guide walks you through setting up AWS infrastructure and GitHub Actions for deploying the Musical Scales API.

## Quick Start (Automated)

**⚡ Want to automate the setup?** Use the scaffolding script:

```powershell
# 1. Run automated setup (auto-detects everything!)
cd Scaffolding
.\Setup-AWS.ps1

# 2. Configure GitHub secrets (script shows values)
# 3. Push and deploy!
```

The script auto-detects AWS region, GitHub repo, and uses `musical-scales-terraform-state` as the default bucket name. You'll only be prompted if that bucket name is taken.

See [Scaffolding/README.md](Scaffolding/README.md) for details.

---

## Manual Setup (Step-by-Step)

If you prefer to set up resources manually, follow the steps below.

## Prerequisites

- AWS Account with admin access
- GitHub repository
- AWS CLI installed locally
- Terraform installed locally (for testing)
- AWS region: ap-southeast-2 (Sydney)

## Step 1: AWS Account Setup

### 1.1 Create S3 Bucket for Terraform State

```powershell
# Replace with your bucket name (must be globally unique)
$TF_STATE_BUCKET = "your-terraform-state-bucket"
$AWS_REGION = "ap-southeast-2"

aws s3api create-bucket `
  --bucket $TF_STATE_BUCKET `
  --region $AWS_REGION

# Enable versioning (important for Terraform state)
aws s3api put-bucket-versioning `
  --bucket $TF_STATE_BUCKET `
  --versioning-configuration Status=Enabled

# Enable encryption
aws s3api put-bucket-encryption `
  --bucket $TF_STATE_BUCKET `
  --server-side-encryption-configuration '{
    "Rules": [{
      "ApplyServerSideEncryptionByDefault": {
        "SSEAlgorithm": "AES256"
      }
    }]
  }'

# Block public access
aws s3api put-public-access-block `
  --bucket $TF_STATE_BUCKET `
  --public-access-block-configuration `
    BlockPublicAcls=true,IgnorePublicAcls=true,BlockPublicPolicy=true,RestrictPublicBuckets=true
```

### 1.2 State Locking

**State locking is automatically enabled using S3's native `use_lockfile` feature** (Terraform 1.8.0+). This means:
- ✅ No DynamoDB table needed
- ✅ State locking is built-in and free
- ✅ Prevents concurrent modifications
- ✅ Simpler setup

The Terraform configuration uses `use_lockfile = true` which creates a `.tflock` file in S3 alongside the state file.

## Step 2: GitHub OIDC Provider Setup

### 2.1 Create OIDC Provider in AWS

```powershell
# Get your GitHub organization/username
$GITHUB_ORG = "your-github-username"

# Create OIDC provider
aws iam create-open-id-connect-provider `
  --url https://token.actions.githubusercontent.com `
  --client-id-list sts.amazonaws.com `
  --thumbprint-list 6938fd4d98bab03faadb97b34396831e3780aea1
```

### 2.2 Create IAM Role for GitHub Actions

Create a file named `github-actions-trust-policy.json`:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Federated": "arn:aws:iam::YOUR_ACCOUNT_ID:oidc-provider/token.actions.githubusercontent.com"
      },
      "Action": "sts:AssumeRoleWithWebIdentity",
      "Condition": {
        "StringEquals": {
          "token.actions.githubusercontent.com:aud": "sts.amazonaws.com"
        },
        "StringLike": {
          "token.actions.githubusercontent.com:sub": "repo:YOUR_GITHUB_ORG/musical-scales:*"
        }
      }
    }
  ]
}
```

Replace:
- `YOUR_ACCOUNT_ID` with your AWS account ID
- `YOUR_GITHUB_ORG` with your GitHub username/org

```powershell
# Get your AWS account ID
$ACCOUNT_ID = aws sts get-caller-identity --query Account --output text

# Update the trust policy file with your values
(Get-Content github-actions-trust-policy.json) `
  -replace 'YOUR_ACCOUNT_ID', $ACCOUNT_ID `
  -replace 'YOUR_GITHUB_ORG', $GITHUB_ORG |
  Set-Content github-actions-trust-policy.json

# Create IAM role
aws iam create-role `
  --role-name GitHubActionsMusicalScales `
  --assume-role-policy-document file://github-actions-trust-policy.json `
  --description "Role for GitHub Actions to deploy Musical Scales API"
```

### 2.3 Attach Policies to IAM Role

Create a file named `github-actions-policy.json`:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::YOUR_TF_STATE_BUCKET/*",
        "arn:aws:s3:::YOUR_TF_STATE_BUCKET"
      ]
    },
    {
      "Effect": "Allow",
      "Action": [
        "lambda:*",
        "apigateway:*",
        "iam:CreateRole",
        "iam:DeleteRole",
        "iam:GetRole",
        "iam:PassRole",
        "iam:AttachRolePolicy",
        "iam:DetachRolePolicy",
        "iam:PutRolePolicy",
        "iam:DeleteRolePolicy",
        "iam:GetRolePolicy",
        "logs:*",
        "cloudwatch:*"
      ],
      "Resource": "*"
    }
  ]
}
```

**Note:** S3 `DeleteObject` permission is needed for the `.tflock` file management.

Replace placeholders and create policy:

```powershell
(Get-Content github-actions-policy.json) `
  -replace 'YOUR_TF_STATE_BUCKET', $TF_STATE_BUCKET |
  Set-Content github-actions-policy.json

# Create and attach policy
aws iam put-role-policy `
  --role-name GitHubActionsMusicalScales `
  --policy-name GitHubActionsMusicalScalesPolicy `
  --policy-document file://github-actions-policy.json
```

## Step 3: GitHub Secrets Configuration

Add the following secrets to your GitHub repository (Settings → Secrets and variables → Actions → New repository secret):

### Required Secrets

1. **AWS_REGION**
   ```
   ap-southeast-2
   ```

2. **AWS_ROLE_ARN**
   ```powershell
   # Get the role ARN
   aws iam get-role --role-name GitHubActionsMusicalScales --query 'Role.Arn' --output text
   ```
   Example: `arn:aws:iam::123456789012:role/GitHubActionsMusicalScales`

3. **TF_STATE_BUCKET**
   ```
   your-terraform-state-bucket
   ```

4. **ENVIRONMENT**
   ```
   dev
   ```
   (Can be: dev, staging, or prod)

## Step 4: Verify Setup

### 4.1 Test AWS CLI Access

If not already authenticated:
```powershell
aws login
```

Then verify:
```powershell
aws sts get-caller-identity
```

### 4.2 Test S3 Bucket

```powershell
aws s3 ls s3://$TF_STATE_BUCKET
```

## Step 5: Initial Deployment

### Option A: Deploy via GitHub Actions (Recommended)

1. Push to `main` branch
2. GitHub Actions will automatically:
   - Build the .NET Lambda package
   - Run Terraform
   - Deploy to AWS

### Option B: Deploy Manually (for testing)

```powershell
# 1. Build and package Lambda
cd MusicalScales.Api
dotnet lambda package --configuration Release --framework net8.0 --output-package ../lambda-package.zip

# 2. Initialize Terraform
cd ../terraform
terraform init `
  -backend-config="bucket=$TF_STATE_BUCKET" `
  -backend-config="key=musical-scales/dev/terraform.tfstate" `
  -backend-config="region=$AWS_REGION"

# 3. Plan deployment
terraform plan `
  -var="aws_region=$AWS_REGION" `
  -var="environment=dev" `
  -out=tfplan

# 4. Apply (if plan looks good)
terraform apply -auto-approve tfplan

# 5. Get API key
$API_KEY = terraform output -raw api_key_value
Write-Host "Your API Key: $API_KEY"

# 6. Test API
$API_URL = terraform output -raw api_gateway_url
curl -H "x-api-key: $API_KEY" "$API_URL/health"
```

## Step 6: Using the API

### Get API URL and Key

```powershell
cd terraform
terraform output api_gateway_url
terraform output api_key_value
```

### Test Endpoints

```powershell
$API_URL = "your-api-gateway-url"
$API_KEY = "your-api-key"

# Health check
curl -H "x-api-key: $API_KEY" "$API_URL/health"

# Get all scales
curl -H "x-api-key: $API_KEY" "$API_URL/api/scales"

# Get specific scale
curl -H "x-api-key: $API_KEY" "$API_URL/api/scales/{scale-id}"

# Search scales
curl -H "x-api-key: $API_KEY" "$API_URL/api/scales/search?name=Major"
```

## Step 7: Monitoring and Logs

### View Lambda Logs

```powershell
# Get log group name
cd terraform
$LOG_GROUP = terraform output -raw cloudwatch_log_group_lambda

# View recent logs
aws logs tail $LOG_GROUP --follow
```

### View API Gateway Logs

```powershell
# Get log group name
cd terraform
$LOG_GROUP = terraform output -raw cloudwatch_log_group_api_gateway

# View recent logs
aws logs tail $LOG_GROUP --follow
```

### View CloudWatch Alarms

```powershell
aws cloudwatch describe-alarms --alarm-name-prefix musical-scales-api
```

## Troubleshooting

### Issue: Terraform state lock error

**Solution:** S3 native locking uses `.tflock` files. Check for orphaned lock files:
```powershell
# List state files and locks
aws s3 ls s3://$TF_STATE_BUCKET/musical-scales/dev/

# Remove orphaned lock file if needed
aws s3 rm s3://$TF_STATE_BUCKET/musical-scales/dev/terraform.tfstate.tflock
```

### Issue: API returns 403 Forbidden

**Solution:** Ensure you're passing the API key:
```powershell
curl -H "x-api-key: YOUR_API_KEY" "$API_URL/api/scales"
```

### Issue: Lambda timeout or errors

**Solution:** Check CloudWatch Logs:
```powershell
aws logs tail /aws/lambda/musical-scales-api-dev --follow
```

## Clean Up Resources

To delete all AWS resources:

```powershell
cd terraform
terraform destroy `
  -var="aws_region=$AWS_REGION" `
  -var="environment=dev"
```

**Note:** This will NOT delete:
- S3 bucket (Terraform state)
- IAM roles/policies

Delete these manually if needed:
```powershell
aws s3 rb s3://$TF_STATE_BUCKET --force
aws iam delete-role-policy --role-name GitHubActionsMusicalScales --policy-name GitHubActionsMusicalScalesPolicy
aws iam delete-role --role-name GitHubActionsMusicalScales
```

## Cost Estimate

With typical POC usage (<10,000 requests/month):

- **Lambda**: $0 (within free tier: 1M requests/month, 400K GB-seconds/month)
- **API Gateway**: ~$0.35/month (first 1M requests free for 12 months, then $3.50/1M requests)
- **CloudWatch Logs**: ~$0.50/month (5GB ingestion free, then $0.50/GB)
- **S3**: ~$0.02/month (Terraform state storage only)

**Total**: ~$0.87/month (after free tier expires)

**Note:** State locking is free with S3's native `use_lockfile` feature - no DynamoDB charges!

## Next Steps

1. ✅ Set up custom domain (see DEPLOYMENT.md Option 4)
2. ✅ Add multiple environments (dev, staging, prod)
3. ✅ Set up CloudWatch dashboards
4. ✅ Add SNS notifications for alarms
5. ✅ Implement CI/CD for multiple branches
6. ✅ Add Cognito authentication (see DEPLOYMENT.md Option 5)
