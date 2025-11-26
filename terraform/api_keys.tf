# API Gateway API Key
resource "aws_api_gateway_api_key" "main" {
  name        = "${var.function_name}-${var.environment}-key"
  description = "API key for Musical Scales API - ${var.environment}"
  enabled     = true
}

# API Gateway Usage Plan
resource "aws_api_gateway_usage_plan" "main" {
  name        = "${var.function_name}-${var.environment}-usage-plan"
  description = "Usage plan for Musical Scales API - ${var.environment}"

  api_stages {
    api_id = aws_api_gateway_rest_api.main.id
    stage  = aws_api_gateway_stage.main.stage_name
  }

  quota_settings {
    limit  = var.api_key_usage_limit
    period = "DAY"
  }

  throttle_settings {
    burst_limit = var.api_throttle_burst_limit
    rate_limit  = var.api_throttle_rate_limit
  }
}

# Associate API Key with Usage Plan
resource "aws_api_gateway_usage_plan_key" "main" {
  key_id        = aws_api_gateway_api_key.main.id
  key_type      = "API_KEY"
  usage_plan_id = aws_api_gateway_usage_plan.main.id
}
