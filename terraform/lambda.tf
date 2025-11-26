# IAM role for Lambda execution
resource "aws_iam_role" "lambda_execution" {
  name = "${var.function_name}-${var.environment}-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

# Attach basic Lambda execution policy
resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.lambda_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# Additional policy for CloudWatch Logs and DynamoDB
resource "aws_iam_role_policy" "lambda_logging" {
  name = "${var.function_name}-${var.environment}-logging-policy"
  role = aws_iam_role.lambda_execution.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "logs:CreateLogGroup",
          "logs:CreateLogStream",
          "logs:PutLogEvents"
        ]
        Resource = "arn:aws:logs:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:log-group:/aws/lambda/${var.function_name}-${var.environment}:*"
      },
      {
        Effect = "Allow"
        Action = [
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem",
          "dynamodb:Scan",
          "dynamodb:Query"
        ]
        Resource = aws_dynamodb_table.scales.arn
      }
    ]
  })
}

# Lambda function
resource "aws_lambda_function" "api" {
  function_name = "${var.function_name}-${var.environment}"
  role          = aws_iam_role.lambda_execution.arn
  handler       = "MusicalScales.Api" # Must match assembly name, not function name
  runtime       = "dotnet8"
  memory_size   = 512
  timeout       = 30

  # Use local zip file instead of S3
  filename         = "../lambda-package.zip"
  source_code_hash = filebase64sha256("../lambda-package.zip")

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = title(var.environment)
      DynamoDB__TableName    = aws_dynamodb_table.scales.name
    }
  }

  depends_on = [
    aws_iam_role_policy_attachment.lambda_basic_execution,
    aws_iam_role_policy.lambda_logging,
    aws_cloudwatch_log_group.lambda
  ]
}

# Lambda permission for API Gateway
resource "aws_lambda_permission" "api_gateway" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.api.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_api_gateway_rest_api.main.execution_arn}/*/*/*"
}
