# Musical Scales API

A modern .NET 8 Web API that demonstrates how to represent musical scales in an object-oriented way. This API provides CRUD operations for managing musical scales, intervals, and pitches with proper music theory modeling.

This repository serves as a demonstration of:

- **Object-Oriented Music Theory**: Representing musical concepts (scales, intervals, pitches, accidentals) as proper C# classes
- **Modern .NET Patterns**: Built with .NET 8, Entity Framework Core, and contemporary API design practices
- **CRUD Operations**: Full create, read, update, and delete functionality for musical scales
- **API Documentation**: Complete OpenAPI/Swagger documentation for all endpoints
- **Serverless Architecture**: AWS Lambda + API Gateway + DynamoDB for production deployment
- **Dual Database Support**: SQLite for local development, DynamoDB for AWS Lambda

The main objective is to showcase how complex musical relationships can be modeled and manipulated through code, making music theory concepts accessible programmatically.

## How to Run the API

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Running the Application

1. **Run the application**
   ```bash
   dotnet run
   ```

2. **Access the API**
   - **Swagger UI**: http://localhost:5000
   - **HTTPS**: https://localhost:5001
   - **Health Check**: http://localhost:5000/health

3. **Populate the Database** (optional but recommended)

   The API starts with an empty database. To populate it with sample scales, use the provided PowerShell script:

   ```powershell
   # Populate local database with all sample scales
   .\Populate-Scales.ps1

   # Or populate a specific environment
   .\Populate-Scales.ps1 -Environment local

   # For AWS dev environment (requires API key)
   .\Populate-Scales.ps1 -Environment dev -ApiKey "your-api-key-here"
   ```

   This will populate the database with 11 pre-defined scales:
   - Major (Ionian)
   - Natural Minor (Aeolian)
   - Harmonic Minor
   - Melodic Minor
   - Dorian
   - Phrygian
   - Lydian
   - Mixolydian
   - Locrian
   - Pentatonic Major
   - Pentatonic Minor

   The scales are loaded from `MusicalScales.Api/Data/SeedData/Scales/` and created via the API's `POST /api/scales` endpoint.

## Database Architecture

The API uses a **dual database strategy**:

### Local Development (SQLite)
- **Database**: SQLite file-based database
- **Location**: `musicscales.db` in the project directory
- **Benefits**: Simple setup, no external dependencies, easy to reset
- **Use Case**: Local development and testing

### AWS Lambda (DynamoDB)
- **Database**: AWS DynamoDB serverless NoSQL database
- **Detection**: Automatically uses DynamoDB when `AWS_EXECUTION_ENV` environment variable is present
- **Benefits**: Serverless, scalable, persistent storage, pay-per-use pricing
- **Free Tier**: 25 GB storage, 25 WCU, 25 RCU per month
- **Table Structure**:
  - **Partition Key**: `Id` (GUID)
  - **Attributes**: `Metadata` (JSON), `Intervals` (JSON), `CreatedAt`, `UpdatedAt`, `IntervalsHash`, `NamesSearchable`

The database provider is automatically selected based on the runtime environment, requiring no code changes or configuration.

### Available Endpoints

- `GET /api/scales` - Get all scales
- `GET /api/scales/{id}` - Get scale by ID
- `GET /api/scales/search?name={name}` - Search scales by name
- `POST /api/scales/{id}/pitches` - Generate scale pitches from root pitch
- `POST /api/scales` - Create new scale
- `PUT /api/scales/{id}` - Update existing scale
- `DELETE /api/scales/{id}` - Delete scale

Visit the Swagger UI at the root URL for interactive documentation and testing.

## Development & Testing

### Testing

This project includes both unit tests and integration tests to ensure comprehensive code coverage:

#### Unit Tests (110+ tests)
- **Service Layer Tests**: Complete coverage of PitchService, IntervalService, ScaleService, and DatabaseSeeder
- **Mocking & Isolation**: Uses Moq for dependency isolation and proper unit testing
- **Code Coverage**: Tools and instructions for generating coverage reports

For detailed unit testing information, see: 📖 **[MusicalScales.Tests/README.md](MusicalScales.Tests/README.md)**

#### Integration Tests (8 tests)
- **End-to-End API Testing**: Tests all endpoints using WebApplicationFactory
- **JSON Payload Testing**: Uses external JSON files for realistic test data
- **Database Integration**: Tests with in-memory database and seeded data
- **HTTP Semantics**: Validates status codes, headers, and response formats

For detailed integration testing information, see: 📖 **[MusicalScales.IntegrationTests/README.md](MusicalScales.IntegrationTests/README.md)**

### Running Tests
```bash
# Run all tests (110+ unit tests + 8 integration tests)
dotnet test

# Run only unit tests
dotnet test MusicalScales.Tests

# Run only integration tests  
dotnet test MusicalScales.IntegrationTests

# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"
```

**PowerShell Scripts (Recommended):**
```powershell
# From the MusicalScales.Tests directory
.\RunUnitTests.ps1  # Runs unit tests + generates coverage report + opens in browser

# From the root directory  
.\run-integration-tests.ps1  # Runs integration tests (if available)
```

### Continuous Integration
The project includes a streamlined GitHub Actions workflow that automatically:
- ✅ Builds the solution
- ✅ Runs all 110+ unit tests  
- ✅ Runs all 8 integration tests
- ✅ Reports test results with detailed summaries

The workflow runs on every push to `main` branch and on pull requests to `main`.

### Project Structure
```
musical-scales/
├── MusicalScales.Api/               # Main API project
│   ├── Controllers/                 # API controllers
│   ├── Services/                   # Business logic services
│   ├── Models/                     # Domain models
│   ├── Data/                       # Entity Framework context
│   │   └── SeedData/Scales/        # Sample scale JSON files
│   └── Repositories/               # Data access layer
│       ├── IScaleRepository.cs     # Repository interface
│       ├── ScaleRepository.cs      # SQLite implementation (EF Core)
│       └── DynamoDbScaleRepository.cs  # DynamoDB implementation
├── MusicalScales.Tests/             # Unit test project (110+ tests)
│   ├── Services/                   # Service layer tests
│   └── README.md                   # Unit testing documentation
├── MusicalScales.IntegrationTests/ # Integration test project (8 tests)
│   ├── Controllers/                 # API endpoint tests
│   ├── Fixtures/                   # Test setup and data
│   ├── TestData/Scales/            # JSON scale test payloads
│   └── README.md                   # Integration testing documentation
├── terraform/                       # Infrastructure as Code
│   ├── main.tf                     # Main Terraform configuration
│   ├── lambda.tf                   # Lambda function definition
│   ├── api_gateway.tf              # API Gateway configuration
│   ├── dynamodb.tf                 # DynamoDB table definition
│   └── variables.tf                # Terraform variables
├── Scaffolding/                     # AWS setup scripts
│   ├── Setup-AWS.ps1               # Automated AWS resource creation
│   ├── Verify-Setup.ps1            # Validate AWS configuration
│   └── README.md                   # Setup documentation
├── Debugging/                       # Development debugging scripts
│   ├── get-scale-pitches.ps1       # Display scale pitches
│   └── README.md                   # Debugging scripts documentation
├── Populate-Scales.ps1              # Database population script
└── README.md                        # This file
```
