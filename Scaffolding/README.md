# AWS Setup Scaffolding

This folder contains scripts to automate the AWS infrastructure setup for the Musical Scales API.

## Prerequisites

Before running the setup script, ensure you have:

1. **AWS CLI installed**
   ```powershell
   choco install awscli
   ```

2. **AWS credentials configured**
   ```powershell
   aws login
   ```
   Select **ap-southeast-2** as your region when prompted.

That's it! The script auto-detects everything else.

## Quick Start

### 1. Run Setup Script

```powershell
cd Scaffolding
.\Setup-AWS.ps1
```

The script will:
- ✅ Auto-detect AWS region from your AWS CLI config
- ✅ Auto-detect GitHub org/repo from git remote
- ✅ Try default S3 bucket name: `musical-scales-terraform-state`
- ✅ Prompt only if default bucket is taken by someone else
- ✅ Create S3 bucket for Terraform state (with versioning and encryption)
- ✅ Create GitHub OIDC provider for keyless authentication
- ✅ Create IAM role for GitHub Actions
- ✅ Attach necessary policies for deployment
- ✅ Display GitHub secrets to configure

**What gets auto-detected:**
- **AWS Region**: From `aws configure get region`
- **GitHub Org**: From `git config --get remote.origin.url`
- **GitHub Repo**: From `git config --get remote.origin.url`
- **S3 Bucket**: Tries `musical-scales-terraform-state` first
- **Environment**: Defaults to "dev" (can override with `-Environment prod`)

**When you'll be prompted:**
- Only if `musical-scales-terraform-state` is taken by another AWS account

### 2. Configure GitHub Secrets

After the script completes, it will display 4 secrets to add to your GitHub repository:

1. Go to: **Settings → Secrets and variables → Actions → New repository secret**
2. Add each of the 4 secrets shown by the script:
   - `AWS_REGION`
   - `AWS_ROLE_ARN`
   - `TF_STATE_BUCKET`
   - `ENVIRONMENT`

### 3. Deploy

Once secrets are configured:

```powershell
git add .
git commit -m "Configure AWS deployment"
git push origin your-branch

# Create PR and merge to main
# GitHub Actions will automatically deploy!
```

## Files

### Setup-AWS.ps1

Main setup script that creates all AWS resources.

### JSON Templates

The script uses template files for AWS policies:

- **trust-policy-template.json** - IAM role trust policy template
  - Placeholders: `ACCOUNT_ID`, `GITHUB_ORG`, `GITHUB_REPO`
- **role-policy-template.json** - IAM role permissions policy template
  - Placeholder: `TF_STATE_BUCKET`
- **s3-encryption-config.json** - S3 bucket encryption configuration (no placeholders)

These templates ensure proper JSON formatting and make policies easier to review and maintain.

**Features:**
- ✅ **Idempotent**: Safe to run multiple times
- ✅ **Validates** environment variables before starting
- ✅ **Checks** AWS CLI and credentials
- ✅ **Creates resources** only if they don't exist
- ✅ **Colored output** for easy reading

**Options:**
```powershell
# Use defaults (tries musical-scales-terraform-state)
.\Setup-AWS.ps1

# Custom bucket name
.\Setup-AWS.ps1 -TfStateBucket "my-terraform-state-bucket"

# Production environment with custom bucket
.\Setup-AWS.ps1 -TfStateBucket "my-tf-state" -Environment "prod"

# Verbose output
.\Setup-AWS.ps1 -Verbose
```

## Troubleshooting

### "AWS CLI is not installed"

Install AWS CLI:
```powershell
choco install awscli
```

### "AWS credentials are not configured"

Configure credentials:
```powershell
aws login
```
Select **ap-southeast-2** as your region.

### "S3 bucket already exists (owned by someone else)"

The bucket name must be globally unique. Choose a different name when prompted, or:
```powershell
.\Setup-AWS.ps1 -TfStateBucket "my-unique-musical-scales-terraform-state"
```

### "Access Denied" errors

Ensure your AWS credentials have permissions to:
- Create S3 buckets
- Create IAM roles and policies
- Create OIDC providers

You may need an AWS account with admin privileges for initial setup.

### Re-running the script

The script is idempotent - it's safe to run multiple times. It will:
- Skip existing resources
- Update configurations if needed
- Display current status

## What Gets Created

### S3 Bucket
- **Name**: Value from `TF_STATE_BUCKET` in `.env`
- **Purpose**: Store Terraform state files
- **Features**:
  - Versioning enabled
  - Encryption enabled (AES256)
  - Public access blocked

### GitHub OIDC Provider
- **ARN**: `arn:aws:iam::ACCOUNT_ID:oidc-provider/token.actions.githubusercontent.com`
- **Purpose**: Allow GitHub Actions to authenticate without access keys
- **Audience**: `sts.amazonaws.com`

### IAM Role
- **Name**: `GitHubActionsMusicalScales`
- **Purpose**: Role assumed by GitHub Actions for deployment
- **Trust**: Only allows your specific GitHub repository

### IAM Policy
- **Name**: `GitHubActionsMusicalScalesPolicy`
- **Permissions**:
  - S3: Read/write Terraform state
  - Lambda: Full access
  - API Gateway: Full access
  - IAM: Create/manage Lambda execution roles
  - CloudWatch: Logs and monitoring

## Cost

All resources created by this script are free:
- ✅ S3 bucket: ~$0.02/month (minimal Terraform state storage)
- ✅ OIDC provider: Free
- ✅ IAM role and policies: Free

Total setup cost: **~$0.02/month**

## Next Steps

After setup completes:

1. ✅ Configure GitHub secrets (script will show values)
2. ✅ Push your changes
3. ✅ Create PR and merge to main
4. ✅ Watch GitHub Actions deploy your API!

For more details, see:
- [AWS_SETUP.md](../Docs/AWS_SETUP.md) - Full manual setup guide
- [QUICK_DEPLOY.md](../Docs/QUICK_DEPLOY.md) - Quick deployment reference
- [DEPLOYMENT.md](../Docs/DEPLOYMENT.md) - Deployment strategy overview
