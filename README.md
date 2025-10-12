# Musical Scales API

A modern .NET 8 Web API that demonstrates how to represent musical scales in an object-oriented way. This API provides CRUD operations for managing musical scales, intervals, and pitches with proper music theory modeling.

This repository serves as a demonstration of:

- **Object-Oriented Music Theory**: Representing musical concepts (scales, intervals, pitches, accidentals) as proper C# classes
- **Modern .NET Patterns**: Built with .NET 8, Entity Framework Core, and contemporary API design practices
- **CRUD Operations**: Full create, read, update, and delete functionality for musical scales
- **API Documentation**: Complete OpenAPI/Swagger documentation for all endpoints

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

The API will start with some pre-seeded scales (Major and Natural Minor) for immediate testing and exploration.

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
│   └── Repositories/               # Data access layer
├── MusicalScales.Tests/             # Unit test project (110+ tests)
│   ├── Services/                   # Service layer tests
│   └── README.md                   # Unit testing documentation
└── MusicalScales.IntegrationTests/ # Integration test project (8 tests)
    ├── Controllers/                 # API endpoint tests
    ├── Fixtures/                   # Test setup and data
    ├── ApiTestRequests/            # JSON test payloads
    └── README.md                   # Integration testing documentation
```
