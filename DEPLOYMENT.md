# Deployment Strategy for Musical Scales API

This document outlines deployment options for hosting the Musical Scales API on AWS using Terraform for infrastructure management.

## Background

This approach is an evolution from the [numerology project](https://github.com/Alturil/numerology) which uses a simpler GitHub Actions + `dotnet lambda deploy-function` approach without Infrastructure-as-Code (IaC).

### Numerology Approach Pros/Cons

**Pros:**
- ✅ Very simple setup
- ✅ Modern OIDC security (no long-lived credentials)
- ✅ Fast iteration
- ✅ Cost effective (Function URLs are free)

**Cons:**
- ❌ No infrastructure reproducibility
- ❌ No custom domain support
- ❌ No built-in authentication
- ❌ Manual infrastructure setup required
- ❌ Limited API Gateway features

## Terraform Deployment Options

### Option 1: Lambda Function URL (Simplest - No Auth)

**What you get:**
- AWS Lambda with .NET 8
- Function URL (no API Gateway)
- HTTPS endpoint automatically

**Terraform Resources:**
- `aws_lambda_function`
- `aws_iam_role` (Lambda execution role)
- `aws_lambda_function_url`
- `aws_cloudwatch_log_group`

**Pros:**
- Minimal Terraform code (~50 lines)
- Fast deployment
- No API Gateway costs
- Simple CORS configuration

**Cons:**
- ❌ No custom domain
- ❌ No built-in auth
- ❌ No throttling/rate limiting
- ❌ No request transformation
- AWS-generated URL only

**Good for:** Internal tools, POCs, demos

**Estimated Cost:** ~$0/month (free tier)

---

### Option 2: API Gateway HTTP API + Lambda (Simple Auth)

**What you get:**
- Lambda behind API Gateway HTTP API
- JWT authorizers (e.g., Auth0, Cognito)
- Better cost than REST API
- CORS built-in

**Terraform Resources:**
- `aws_lambda_function`
- `aws_iam_role`
- `aws_apigatewayv2_api` (HTTP API)
- `aws_apigatewayv2_stage`
- `aws_apigatewayv2_integration`
- `aws_apigatewayv2_route`
- `aws_apigatewayv2_authorizer` (JWT)

**Pros:**
- Still relatively simple (~100 lines)
- JWT validation built-in
- 70% cheaper than REST API
- Automatic deployments
- Stage support (dev/prod)

**Cons:**
- ❌ No API keys (only JWT)
- ❌ No usage plans
- ❌ No request validation
- Limited throttling options

**Good for:** Modern SPAs with JWT auth, mobile apps

**Estimated Cost:** ~$1/month (low traffic)

---

### Option 3: API Gateway REST API + Lambda + API Keys (Recommended)

**What you get:**
- Lambda behind API Gateway REST API
- API Key authentication
- Usage plans & throttling
- Request/response transformation
- Custom domain support ready

**Terraform Resources:**
- `aws_lambda_function`
- `aws_iam_role`
- `aws_api_gateway_rest_api`
- `aws_api_gateway_resource`
- `aws_api_gateway_method`
- `aws_api_gateway_integration`
- `aws_api_gateway_deployment`
- `aws_api_gateway_stage`
- `aws_api_gateway_api_key`
- `aws_api_gateway_usage_plan`
- `aws_api_gateway_usage_plan_key`

**Pros:**
- ✅ Simple token-based auth (API keys)
- ✅ Rate limiting per key
- ✅ Usage quotas (requests/day)
- ✅ Request validation
- ✅ Easy custom domain addition later
- ✅ Multiple stages (dev/staging/prod)

**Cons:**
- More Terraform code (~150-200 lines)
- More expensive than HTTP API
- API keys less secure than JWT (but simpler for POC)

**Good for:** Third-party integrations, B2B APIs, POC with professional features

**Estimated Cost:** ~$3.50/month (low traffic)

---

### Option 4: Custom Domain + Route53 + ACM Certificate (Production Ready)

**Adds to Option 3:**
- Custom domain (api.yourdomain.com)
- SSL certificate from ACM
- Route53 DNS management

**Additional Terraform Resources:**
- `aws_acm_certificate`
- `aws_acm_certificate_validation`
- `aws_route53_zone` (or data source if exists)
- `aws_route53_record`
- `aws_api_gateway_domain_name`
- `aws_api_gateway_base_path_mapping`

**Pros:**
- ✅ Professional custom domain
- ✅ Free SSL certificate
- ✅ Automatic DNS management
- ✅ Multiple environment subdomains (api-dev, api-staging, api)

**Cons:**
- Requires domain ownership
- DNS propagation time
- Certificate validation delay (first deploy ~5-10 minutes)

**Estimated Cost:** ~$3.50/month (same as Option 3, Route53 zone is $0.50/month)

---

### Option 5: Multiple Auth Methods (Best - Most Flexible)

**Adds to Option 4:**
- Lambda authorizer (custom auth logic)
- Cognito User Pool
- API Keys + JWT support

**Additional Terraform Resources:**
- `aws_cognito_user_pool`
- `aws_cognito_user_pool_client`
- `aws_cognito_user_pool_domain`
- `aws_lambda_function` (authorizer)
- `aws_api_gateway_authorizer`

**Pros:**
- ✅ User management (Cognito)
- ✅ Custom auth logic
- ✅ Social login support
- ✅ Multiple auth strategies
- ✅ MFA support

**Cons:**
- Most complex (~300+ lines)
- Cognito learning curve
- More services to manage
- Overkill for POC

**Good for:** Production applications with user management

**Estimated Cost:** ~$3.50/month (Cognito is free for <50,000 MAU)

---

## Complexity Comparison

| Option | Terraform Lines | Setup Time | Auth Type | Cost/mo* | Custom Domain | Rate Limiting |
|--------|----------------|------------|-----------|----------|---------------|---------------|
| 1. Function URL | ~50 | 10 min | None | $0 | ❌ | ❌ |
| 2. HTTP API + JWT | ~100 | 30 min | JWT | $1 | ✅ | Limited |
| 3. REST API + Keys | ~200 | 1 hour | API Keys | $3.50 | ✅ | ✅ |
| 4. + Custom Domain | ~300 | 2 hours | API Keys | $3.50 | ✅ | ✅ |
| 5. + Cognito | ~400 | 3+ hours | Multi | $3.50 | ✅ | ✅ |

*Assuming low traffic (<1M requests/month, within AWS free tier)

---

## Recommended Approach for Musical Scales API

**Start with Option 3 (REST API + API Keys)**, optionally add Option 4 (Custom Domain) later.

### Why Option 3?

1. **Simple authentication** - Generate API keys, distribute them, done
2. **Rate limiting** - Protect free-tier Lambda from abuse
3. **Professional appearance** - Looks production-ready
4. **Extensible** - Easy to add custom domain or Cognito later
5. **Not overkill** - No user management complexity for a POC
6. **Cost effective** - Still very cheap at ~$3.50/month

### Recommended Terraform Structure

```
terraform/
├── main.tf                 # Provider config & backend
├── lambda.tf               # Lambda function + IAM roles
├── api_gateway.tf          # API Gateway REST API resources
├── api_keys.tf             # API keys + usage plans
├── cloudwatch.tf           # Logs + monitoring
├── outputs.tf              # API URL, API key values
├── variables.tf            # Region, stage, function name, etc.
└── versions.tf             # Terraform/provider version constraints

# Optional (for later):
├── domain.tf               # Custom domain + ACM + Route53
└── cognito.tf              # Cognito user pool (if needed)

# Configuration:
├── terraform.tfvars        # Non-sensitive values
└── .gitignore              # Exclude *.tfvars with secrets
```

### Deployment Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Developer pushes to main branch                          │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. GitHub Actions: Build .NET 8 Lambda Package              │
│    - dotnet publish -c Release                               │
│    - Create deployment ZIP                                   │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. Upload Lambda package to S3                              │
│    - Versioned bucket                                        │
│    - Artifact stored for rollback                            │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. Terraform Plan                                            │
│    - terraform plan -out=tfplan                              │
│    - Review changes                                          │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. Terraform Apply                                           │
│    - terraform apply tfplan                                  │
│    - Create/update Lambda, API Gateway, API Keys            │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ 6. Output Deployment Info                                    │
│    - API Gateway URL                                         │
│    - API Key values                                          │
│    - CloudWatch log group                                    │
└─────────────────────────────────────────────────────────────┘
```

### Required GitHub Secrets

For Option 3:
- `AWS_REGION` - AWS region (e.g., us-east-1)
- `AWS_ROLE` - OIDC IAM role ARN for GitHub Actions
- `TF_STATE_BUCKET` - S3 bucket for Terraform state
- `TF_STATE_KEY` - State file path in S3
- `TF_LOCK_TABLE` - DynamoDB table for state locking

### Implementation Checklist

- [ ] Set up AWS account and configure billing alerts
- [ ] Create S3 bucket for Terraform state
- [ ] Create DynamoDB table for state locking
- [ ] Set up GitHub OIDC provider in AWS
- [ ] Create IAM role for GitHub Actions
- [ ] Write Terraform configuration
- [ ] Create GitHub Actions workflow
- [ ] Configure GitHub Secrets
- [ ] Test deployment to dev environment
- [ ] Document API key management
- [ ] Set up monitoring and alerts

---

## Future Enhancements

Once Option 3 is working:

1. **Add Custom Domain** (Option 4)
   - Register domain or use existing
   - Create ACM certificate
   - Configure Route53
   - Update API Gateway

2. **Add Multiple Environments**
   - Terraform workspaces or separate state files
   - dev.api.domain.com
   - staging.api.domain.com
   - api.domain.com

3. **Add Advanced Monitoring**
   - CloudWatch dashboards
   - X-Ray tracing
   - Alarm on error rates
   - Cost monitoring

4. **Add CI/CD Improvements**
   - Automated testing before deploy
   - Canary deployments
   - Automatic rollback on errors
   - Slack/email notifications

5. **Consider Cognito** (Option 5)
   - If user management needed
   - Social login support
   - MFA for admin users

---

## References

- [AWS Lambda with ASP.NET Core](https://docs.aws.amazon.com/lambda/latest/dg/csharp-handler.html)
- [API Gateway REST API](https://docs.aws.amazon.com/apigateway/latest/developerguide/apigateway-rest-api.html)
- [Terraform AWS Provider](https://registry.terraform.io/providers/hashicorp/aws/latest/docs)
- [Amazon.Lambda.AspNetCoreServer](https://github.com/aws/aws-lambda-dotnet)
- [Original numerology project](https://github.com/Alturil/numerology)
