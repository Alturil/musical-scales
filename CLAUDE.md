# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 8 Web API that models musical theory concepts (scales, intervals, pitches) using object-oriented design. The API provides CRUD operations for musical scales with proper Entity Framework Core integration and comprehensive test coverage.

**Technology Stack:**
- .NET 8 Web API
- Entity Framework Core with SQLite
- xUnit testing framework
- Swagger/OpenAPI documentation

## Project History

This repository was migrated and reworked from an older project originally hosted on GitLab. The original codebase can be found at: https://gitlab.com/Alturil/musicscales

The current version has been modernized with:
- Upgraded to .NET 8
- Comprehensive test suite (110+ unit tests, 8 integration tests)
- Modern API patterns and best practices
- Improved architecture and code organization

## Common Commands

### Running the API
```bash
# Run the API (starts on ports 5000/5001)
dotnet run --project MusicalScales.Api

# Access Swagger UI at http://localhost:5000
# Health check at http://localhost:5000/health
```

### Building
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build MusicalScales.Api
```

### Testing
```bash
# Run all tests (110+ unit tests + 8 integration tests)
dotnet test

# Run only unit tests
dotnet test MusicalScales.Tests

# Run only integration tests
dotnet test MusicalScales.IntegrationTests

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "PitchServiceTests"

# Run specific test method
dotnet test --filter "GetPitch_WithMajorSecond_ReturnsCorrectPitch"
```

### PowerShell Scripts
```powershell
# Run unit tests with coverage report (opens HTML in browser)
.\MusicalScales.Tests\RunUnitTests.ps1
```

### Database Operations
```bash
# Create new migration
dotnet ef migrations add MigrationName --project MusicalScales.Api

# Update database
dotnet ef database update --project MusicalScales.Api

# Drop database (SQLite file)
Remove-Item MusicalScales.Api\musicscales.db
```

## Architecture

### Domain Model (Models/)

The core musical concepts are represented as C# classes:

**Scale**: Contains metadata (names, description, origin) and a list of intervals. Intervals are stored as JSON in the database, not as navigation properties.

**Interval**: Represents a musical interval with:
- `Name` (enum): Unison, Second, Third, Fourth, Fifth, Sixth, Seventh, Octave
- `Quality` (enum): Perfect, Major, Minor, Augmented, Diminished
- `PitchOffset`: Number of pitch classes traversed
- `SemitoneOffset`: Number of semitones (half-steps)

**Pitch**: Represents a musical pitch with:
- `PitchClass`: DiatonicPitchName (C, D, E, F, G, A, B)
- `Accidental`: AccidentalName (Natural, Sharp, Flat, DoubleSharp, DoubleFlat)
- `Octave`: Integer representing octave number

### Service Layer (Services/)

**PitchService**: Handles pitch calculations including:
- Getting a pitch from a starting pitch and interval
- Calculating intervals between pitches
- Transposing pitches by semitones
- Handling accidentals and octave wrapping

**IntervalService**: Manages interval operations:
- Creating intervals from semitone/pitch offsets
- Getting interval inverses
- Adding intervals together
- Normalizing large offsets
- Determining interval quality

**ScaleService**: Orchestrates scale operations:
- Generating scale pitches from root pitch and intervals
- CRUD operations via repository pattern
- Scale validation (names, intervals, metadata)

**DatabaseSeeder**: Seeds the database with Major and Natural Minor scales on startup

### Repository Layer (Repositories/)

**ScaleRepository**: Data access layer for Scale entities using Entity Framework Core
- Implements IScaleRepository interface
- All CRUD operations return async Tasks
- Search by name uses case-insensitive partial matching

### Data Layer (Data/)

**MusicalScalesDbContext**:
- Uses SQLite by default (`musicscales.db`)
- Stores `Intervals` as JSON column (not navigation properties)
- Uses `OwnsOne` for `ScaleMetadata` with custom column names
- Custom value comparers for list properties (Names, Intervals)
- Database seeding handled by `DatabaseSeeder` service, not EF Core seed data

### Controllers (Controllers/)

**ScalesController**: REST API endpoints
- `GET /api/scales` - Get all scales
- `GET /api/scales/{id}` - Get scale by ID
- `GET /api/scales/search?name={name}` - Search by name
- `POST /api/scales/{id}/pitches` - Generate pitches from root
- `POST /api/scales` - Create scale
- `PUT /api/scales/{id}` - Update scale
- `DELETE /api/scales/{id}` - Delete scale

## Testing Strategy

### Unit Tests (MusicalScales.Tests/)

**110+ tests** covering service layer with mocked dependencies:
- PitchServiceTests (39 tests): Pitch calculations and interval operations
- IntervalServiceTests (38 tests): Interval creation, inversion, addition
- ScaleServiceTests (19 tests): CRUD operations with mocked repository
- DatabaseSeederTests (14 tests): Database seeding with in-memory EF context

**Key patterns:**
- Uses Moq for dependency mocking
- FluentAssertions for readable test assertions
- In-memory database for DatabaseSeeder integration tests
- Tests cover edge cases, validation, and error handling

### Integration Tests (MusicalScales.IntegrationTests/)

**8 end-to-end tests** using WebApplicationFactory:
- Tests all API endpoints with real HTTP requests
- Uses in-memory database with seeded data
- External JSON files in `ApiTestRequests/` for POST/PUT payloads
- Validates HTTP status codes, response formats, and JSON serialization

**Test data sources:**
- Seeded scales: Major and Natural Minor (for GET operations)
- JSON files: MajorPentatonicScale.json, MinorPentatonicScale.json, CMajorScale.json (for CREATE/UPDATE)

## Key Implementation Details

### JSON Serialization
- Uses `System.Text.Json` with `JsonStringEnumConverter` for enum serialization
- Property naming policy is `JsonNamingPolicy.CamelCase` (e.g., `pitchOffset` instead of `PitchOffset`)
- Configured in `Program.cs` for both API and serialization

### Database Patterns
- Intervals are stored as JSON, not as separate table rows
- ScaleMetadata uses owned entity type with custom column names
- Custom value comparers required for `IList<string>` and `IList<Interval>` properties
- Database is seeded via `DatabaseSeeder` service in `Program.cs`, not EF migrations

### Service Dependencies
- ScaleService depends on IScaleRepository and IPitchService
- PitchService is self-contained (no dependencies)
- IntervalService is self-contained (no dependencies)
- All services registered as scoped in dependency injection

### Validation
- Scale validation occurs in ScaleService before repository operations
- Scales must have at least one name and one interval
- Names cannot be empty or whitespace

## GitHub Actions CI/CD

### Build and Test Workflow
The `.github/workflows/build-and-test.yml` workflow:
- Runs on push to main and PRs
- Builds solution on ubuntu-latest with .NET 8
- Runs unit tests with trx logger
- Runs integration tests separately
- Uses dorny/test-reporter for test result summaries

### Deployment Workflow
The `.github/workflows/deploy.yml` workflow:
- Runs on push to main branch
- Validates Terraform configuration
- Packages Lambda function
- Deploys to AWS using Terraform
- Tests health endpoint after deployment

## AWS Deployment

This API is deployed to AWS Lambda with API Gateway using Infrastructure as Code (Terraform).

### Automated Setup

For quick AWS setup, use the scaffolding script:

```powershell
# 1. Run setup (fully automated!)
cd Scaffolding
.\Setup-AWS.ps1

# 2. Verify setup (optional)
.\Verify-Setup.ps1

# 3. Configure GitHub secrets (shown by setup script)
```

The script auto-detects AWS region, GitHub repo, and uses a default bucket name. You'll only be prompted if the default bucket is taken.

See [Scaffolding/README.md](Scaffolding/README.md) for details.

### Deployment Architecture

- **Lambda Function**: .NET 8 runtime with ASP.NET Core hosting
- **API Gateway**: REST API with API Key authentication
- **CloudWatch**: Logs and monitoring with alarms
- **S3**: Terraform state storage with native locking
- **IAM**: GitHub OIDC provider for keyless authentication

### Deployment Docs

- [Scaffolding/README.md](Scaffolding/README.md) - Automated setup scripts
- [AWS_SETUP.md](Docs/AWS_SETUP.md) - Full manual setup guide
- [QUICK_DEPLOY.md](Docs/QUICK_DEPLOY.md) - Quick deployment reference
- [DEPLOYMENT.md](Docs/DEPLOYMENT.md) - Deployment strategy and options

## Project Structure

```
musical-scales/
├── MusicalScales.Api/               # Main API project
│   ├── Controllers/                 # API endpoints
│   ├── Services/                    # Business logic (Pitch, Interval, Scale services)
│   ├── Models/                      # Domain models (Scale, Pitch, Interval)
│   │   └── Enums/                   # Enums for pitch names, interval qualities, etc.
│   ├── Data/                        # EF Core DbContext
│   ├── Repositories/                # Data access layer
│   └── Program.cs                   # Application startup and DI configuration
├── MusicalScales.Tests/             # Unit tests (110+ tests)
│   ├── Services/                    # Service layer tests
│   └── RunUnitTests.ps1            # PowerShell script for coverage reports
├── MusicalScales.IntegrationTests/  # Integration tests (8 tests)
│   ├── Controllers/                 # API endpoint tests
│   ├── Fixtures/                    # Test setup (WebApplicationFactory)
│   └── ApiTestRequests/            # JSON test payloads
├── Scaffolding/                     # AWS setup automation
│   ├── Setup-AWS.ps1               # Automated AWS infrastructure setup
│   ├── Verify-Setup.ps1            # Verify AWS setup is correct
│   └── README.md                   # Scaffolding documentation
├── terraform/                       # Infrastructure as Code
│   ├── main.tf                     # Terraform backend and provider config
│   ├── lambda.tf                   # Lambda function and IAM roles
│   ├── api_gateway.tf              # API Gateway configuration
│   ├── api_keys.tf                 # API keys and usage plans
│   ├── cloudwatch.tf               # Logging and alarms
│   ├── outputs.tf                  # Output values (URLs, API keys)
│   └── variables.tf                # Input variables
├── .github/workflows/               # GitHub Actions
│   ├── build-and-test.yml          # CI workflow
│   └── deploy.yml                  # CD workflow (deploys to AWS)
├── Docs/                            # Documentation
│   ├── AWS_SETUP.md                # AWS setup documentation
│   ├── QUICK_DEPLOY.md             # Quick deployment reference
│   └── DEPLOYMENT.md               # Deployment strategy guide
├── .env.example                     # Environment variables template
└── musical-scales.sln              # Solution file
```
