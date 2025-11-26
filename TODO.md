# Technical Debt and Improvements TODO

## Critical Issues
None identified.

## Should Fix

### 1. Add DTOs/ViewModels for API Endpoints
**File**: `ScalesController.cs`

The controller directly exposes domain models (`Scale`, `Pitch`, `Interval`), which creates several issues:
- **Over-posting vulnerability**: Clients could potentially set `CreatedAt`, `UpdatedAt`, or `Id` fields
- **Tight coupling**: API contract is tightly bound to database schema
- **Difficult API versioning**: Can't easily shape responses differently for different API versions

**Action**:
- Create DTO classes (e.g., `CreateScaleDto`, `UpdateScaleDto`, `ScaleResponseDto`)
- Update controller methods to accept/return DTOs
- Map between DTOs and domain models in service layer or use AutoMapper

## Consider

### 2. Reconsider Storing Intervals as JSON
**File**: `MusicalScalesDbContext.cs:54-58`

Storing intervals as JSON significantly limits querying capabilities:
- Can't query scales by interval properties (e.g., "find all scales with a Perfect Fifth")
- Can't use SQL joins or indexes on intervals
- Makes data harder to maintain if interval structure changes

**Decision Needed**: Is interval-based querying needed? If yes, create separate `Intervals` table with foreign key to `Scales`.

**Tradeoff**: Query flexibility vs simplicity. Current approach is acceptable for demo/learning project.

### 3. Move Database Seeding to Background Service
**File**: `Program.cs:85-89`

The application blocks startup waiting for database operations:
```csharp
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}
```

**Issues**:
- Delays startup which affects container health checks and deployment
- Not ideal for production environments

**Action**: Consider using `IHostedService` or separate migration process for production deployments.

**Note**: Current approach is fine for demos/local development.

## Infrastructure & Deployment

### 1. ✅ Implement AWS Deployment Pipeline with Terraform ✅
**Status**: COMPLETED

**Implementation**:
- ✅ Terraform state backend (S3 bucket with versioning and encryption)
- ✅ GitHub OIDC provider for keyless AWS authentication
- ✅ Lambda function (.NET 8 runtime) with API Gateway integration
- ✅ DynamoDB table for persistent data storage
- ✅ API Gateway REST API with API key authentication
- ✅ Usage plans with rate limiting (50 req/sec, 100 burst, 1000/day quota)
- ✅ CloudWatch logs and monitoring with alarms
- ✅ GitHub Actions workflow for automated CI/CD
- ✅ Automated setup scripts (Setup-AWS.ps1, Verify-Setup.ps1)

**Future Enhancements**:
- Add custom domain with Route53 + ACM certificate
- Add multiple environments (dev/staging/prod)
- Consider Cognito for user management (if needed)

**Documentation**: See `Docs/DEPLOYMENT.md` and `Scaffolding/README.md`

## Good Practices to Maintain

These are correctly implemented and should be preserved:

1. **Validation in Service Layer** (ScaleService.cs:79-96) - Business validation properly separated from controllers
2. **Repository Pattern** - Clean abstraction between data access and business logic
3. **Proper Async/Await Throughout** - All data operations properly use async patterns
4. **Comprehensive Test Coverage** - 110+ unit tests with mocking and 8 integration tests
5. **Value Comparers for Collections** (MusicalScalesDbContext.cs:42-45, 60-63) - Properly implemented for EF Core change tracking
6. **`public partial class Program {}`** (Program.cs:101) - Enables WebApplicationFactory testing

## Severity Assessment

- **Critical**: None
- **Infrastructure**: Set up AWS deployment pipeline with Terraform
- **Should Fix**: Consider DTOs for API endpoints
- **Consider**: JSON storage strategy if querying intervals becomes important
- **Acceptable for Demo**: Blocking startup seeding

## Completed

- ✅ **Remove Dead Code in DbContext** - Removed unused `SeedData` method and its call from `MusicalScalesDbContext.cs`
- ✅ **Use camelCase JSON Convention** - Changed `PropertyNamingPolicy` from `null` to `JsonNamingPolicy.CamelCase` in `Program.cs`
- ✅ **Implement AWS Deployment Pipeline with Terraform** - Completed Option 3 (API Gateway REST API + Lambda + API Keys + DynamoDB) with full Terraform configuration
- ✅ **Add DynamoDB for Persistent Storage** - Implemented dual database strategy:
  - SQLite for local development (file-based, easy to reset)
  - DynamoDB for AWS Lambda (serverless, persistent, scalable)
  - Automatic environment detection via `AWS_EXECUTION_ENV`
  - Created `DynamoDbScaleRepository` implementation
  - Added Terraform configuration for DynamoDB table with PAY_PER_REQUEST billing
  - Updated IAM permissions for Lambda to access DynamoDB
  - All 118 tests passing (110 unit + 8 integration)

## Notes

For a learning/demo project showcasing music theory OOP, current tradeoffs are reasonable.

**For production deployment, prioritize:**
1. Set up AWS deployment infrastructure (see Docs/DEPLOYMENT.md)
2. Add DTOs for API endpoints
3. Reconsider JSON storage strategy if interval-based queries become important

**Deployment**: See `Docs/DEPLOYMENT.md` for comprehensive analysis of AWS deployment options with Terraform.
