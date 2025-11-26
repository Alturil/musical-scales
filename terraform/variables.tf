variable "aws_region" {
  description = "AWS region to deploy resources"
  type        = string
  default     = "ap-southeast-2"
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "function_name" {
  description = "Lambda function name"
  type        = string
  default     = "musical-scales-api"
}

variable "api_throttle_burst_limit" {
  description = "API Gateway burst limit"
  type        = number
  default     = 100
}

variable "api_throttle_rate_limit" {
  description = "API Gateway rate limit (requests per second)"
  type        = number
  default     = 50
}

variable "api_key_usage_limit" {
  description = "API key usage quota per day"
  type        = number
  default     = 1000
}

variable "tags" {
  description = "Common tags to apply to all resources"
  type        = map(string)
  default = {
    Project    = "MusicalScales"
    ManagedBy  = "Terraform"
    Repository = "musical-scales"
  }
}
