provider "aws" {
  region = var.aws_region

  default_tags {
    tags = var.tags
  }
}

# Backend configuration should be provided via backend config file or command line
# Example: terraform init -backend-config="bucket=my-terraform-state"
#
# S3 native state locking (use_lockfile) is used instead of DynamoDB
# This is simpler and requires no additional AWS resources
terraform {
  backend "s3" {
    # bucket         = "terraform-state-bucket"  # Provided via -backend-config
    # key            = "musical-scales/terraform.tfstate"  # Provided via -backend-config
    # region         = "us-east-1"  # Provided via -backend-config
    encrypt      = true
    use_lockfile = true # S3 native locking (Terraform 1.8.0+)
  }
}

# Data source to get current AWS account ID
data "aws_caller_identity" "current" {}

# Data source to get current AWS region
data "aws_region" "current" {}
