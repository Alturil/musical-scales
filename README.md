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

### Unit Tests
This project includes a comprehensive unit testing suite with 110+ tests covering all service layer functionality. The test suite includes:

- **Service Layer Tests**: Complete coverage of PitchService, IntervalService, ScaleService, and DatabaseSeeder
- **Mocking & Isolation**: Uses Moq for dependency isolation and proper unit testing
- **Integration Testing**: Database seeding and Entity Framework operations
- **Code Coverage**: Tools and instructions for generating coverage reports

For detailed testing information, setup instructions, and coverage reporting, see:
📖 **[MusicalScales.Tests/README.md](MusicalScales.Tests/README.md)**

### Running Tests
```bash
# Run all tests (110 tests pass)
dotnet test

# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"
```

**PowerShell Script (Recommended):**
```powershell
# From the MusicalScales.Tests directory
.\RunUnitTests.ps1  # Runs tests + generates coverage report + opens in browser
```

### Project Structure
```
musical-scales/
├── MusicalScales.Api/          # Main API project
│   ├── Controllers/            # API controllers
│   ├── Services/              # Business logic services
│   ├── Models/                # Domain models
│   ├── Data/                  # Entity Framework context
│   └── Repositories/          # Data access layer
└── MusicalScales.Tests/       # Unit test project
    ├── Services/              # Service layer tests
    └── README.md             # Detailed testing documentation
```
