# DynamoDB table for Musical Scales
resource "aws_dynamodb_table" "scales" {
  name           = "musical-scales-${var.environment}"
  billing_mode   = "PAY_PER_REQUEST" # On-demand pricing (free tier: 25 WCU, 25 RCU)
  hash_key       = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  # Enable point-in-time recovery for data protection
  point_in_time_recovery {
    enabled = true
  }

  # Enable server-side encryption
  server_side_encryption {
    enabled = true
  }

  tags = {
    Name        = "musical-scales-${var.environment}"
    Environment = var.environment
    Project     = "MusicalScales"
    Repository  = "musical-scales"
    ManagedBy   = "Terraform"
  }
}
