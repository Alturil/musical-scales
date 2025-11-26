output "api_gateway_url" {
  description = "API Gateway invoke URL"
  value       = aws_api_gateway_stage.main.invoke_url
}

output "api_gateway_stage" {
  description = "API Gateway stage name"
  value       = aws_api_gateway_stage.main.stage_name
}

output "api_key_id" {
  description = "API Gateway API Key ID"
  value       = aws_api_gateway_api_key.main.id
}

output "api_key_value" {
  description = "API Gateway API Key value (sensitive)"
  value       = aws_api_gateway_api_key.main.value
  sensitive   = true
}

output "lambda_function_name" {
  description = "Lambda function name"
  value       = aws_lambda_function.api.function_name
}

output "lambda_function_arn" {
  description = "Lambda function ARN"
  value       = aws_lambda_function.api.arn
}

output "cloudwatch_log_group_lambda" {
  description = "CloudWatch Log Group for Lambda"
  value       = aws_cloudwatch_log_group.lambda.name
}

output "cloudwatch_log_group_api_gateway" {
  description = "CloudWatch Log Group for API Gateway"
  value       = aws_cloudwatch_log_group.api_gateway.name
}

output "usage_plan_id" {
  description = "API Gateway Usage Plan ID"
  value       = aws_api_gateway_usage_plan.main.id
}

output "region" {
  description = "AWS region"
  value       = data.aws_region.current.name
}

output "account_id" {
  description = "AWS account ID"
  value       = data.aws_caller_identity.current.account_id
}

output "api_documentation" {
  description = "API documentation and usage instructions"
  value       = <<-EOT

    🎵 Musical Scales API Deployed Successfully!

    📍 API Gateway URL: ${aws_api_gateway_stage.main.invoke_url}
    🔑 API Key ID: ${aws_api_gateway_api_key.main.id}

    📝 To retrieve your API key value:
       aws apigateway get-api-key --api-key ${aws_api_gateway_api_key.main.id} --include-value --query 'value' --output text

    🚀 Usage Example:
       curl -H "x-api-key: YOUR_API_KEY" ${aws_api_gateway_stage.main.invoke_url}/api/scales

    📊 CloudWatch Logs:
       Lambda: ${aws_cloudwatch_log_group.lambda.name}
       API Gateway: ${aws_cloudwatch_log_group.api_gateway.name}

    💰 Usage Limits:
       - ${var.api_key_usage_limit} requests per day
       - ${var.api_throttle_rate_limit} requests per second
       - ${var.api_throttle_burst_limit} burst limit
  EOT
}
