# Quick Deploy Reference

This is a condensed reference for developers who have already completed the initial AWS setup (see AWS_SETUP.md).

## Required GitHub Secrets

Configure these in: **Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Example Value | Description |
|------------|---------------|-------------|
| `AWS_REGION` | `ap-southeast-2` | AWS region for deployment |
| `AWS_ROLE_ARN` | `arn:aws:iam::123456789012:role/GitHubActionsMusicalScales` | IAM role ARN for OIDC authentication |
| `TF_STATE_BUCKET` | `my-terraform-state` | S3 bucket for Terraform state |
| `ENVIRONMENT` | `dev` | Environment name (dev/staging/prod) |

**Note:** State locking uses S3's native `use_lockfile` feature - no DynamoDB needed!

## Get Secret Values

```powershell
# Get AWS Role ARN
aws iam get-role --role-name GitHubActionsMusicalScales --query 'Role.Arn' --output text

# Verify S3 bucket exists
aws s3 ls s3://your-terraform-state-bucket
```

## Deploy to AWS

### Automatic (via GitHub Actions)
1. Push to `main` branch
2. GitHub Actions runs automatically
3. Check "Actions" tab for deployment status

### Manual (local testing)
```powershell
# 1. Build and test
dotnet restore
dotnet build --configuration Release
dotnet test

# 2. Package Lambda
cd MusicalScales.Api
dotnet lambda package --configuration Release --framework net8.0 --output-package ../lambda-package.zip

# 3. Deploy with Terraform
cd ../terraform
terraform init `
  -backend-config="bucket=YOUR_TF_STATE_BUCKET" `
  -backend-config="key=musical-scales/dev/terraform.tfstate" `
  -backend-config="region=ap-southeast-2"

terraform apply `
  -var="aws_region=ap-southeast-2" `
  -var="environment=dev"
```

## Get API Credentials

```powershell
cd terraform

# Get API URL
terraform output api_gateway_url

# Get API Key (sensitive - masked)
terraform output api_key_value
```

## Test the API

```powershell
# Set variables
cd terraform
$API_URL = terraform output -raw api_gateway_url
$API_KEY = terraform output -raw api_key_value

# Health check
curl -H "x-api-key: $API_KEY" "$API_URL/health"

# Get all scales
curl -H "x-api-key: $API_KEY" "$API_URL/api/scales"

# Swagger UI
Write-Host $API_URL
```

## View Logs

```powershell
# Lambda logs
cd terraform
$LOG_GROUP = terraform output -raw cloudwatch_log_group_lambda
aws logs tail $LOG_GROUP --follow

# API Gateway logs
cd terraform
$LOG_GROUP = terraform output -raw cloudwatch_log_group_api_gateway
aws logs tail $LOG_GROUP --follow
```

## Common Commands

```powershell
# Check deployment status
cd terraform
terraform show

# Update configuration
terraform plan -var="lambda_memory_size=1024"
terraform apply -var="lambda_memory_size=1024"

# Destroy everything (careful!)
terraform destroy

# Create new API key
terraform apply -target=aws_api_gateway_api_key.main -replace
```

## Troubleshooting

### 403 Forbidden
- Ensure you're passing the API key header: `-H "x-api-key: YOUR_KEY"`

### Lambda timeout
- Check logs: `aws logs tail /aws/lambda/musical-scales-api-dev --follow`
- Increase timeout in `terraform/variables.tf`

### Terraform state lock error
- S3 native locking uses `.tflock` files
- If stuck, check S3 for orphaned lock files: `aws s3 ls s3://YOUR_TF_STATE_BUCKET/musical-scales/dev/`
- Remove manually if needed: `aws s3 rm s3://YOUR_TF_STATE_BUCKET/musical-scales/dev/terraform.tfstate.tflock`

### Deployment fails
- Check GitHub Actions logs
- Verify all 4 secrets are set correctly
- Ensure IAM role has proper permissions (including S3 DeleteObject for lock files)

## URLs

- **Terraform Documentation**: `terraform/`
- **Full Setup Guide**: `AWS_SETUP.md`
- **Deployment Options**: `DEPLOYMENT.md`
