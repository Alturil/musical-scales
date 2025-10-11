# Musical Scales Test Project

This document describes the unit testing project that was created for the Musical Scales API.

## Project Structure

- **MusicalScales.Tests/** - xUnit test project with comprehensive unit tests
  - **Services/PitchServiceTests.cs** - Tests for pitch-related operations
  - **Services/IntervalServiceTests.cs** - Tests for interval calculations  
  - **Services/ScaleServiceTests.cs** - Tests for scale service with mocked dependencies
  - **Services/DatabaseSeederTests.cs** - Tests for database seeding functionality

## Test Coverage

### PitchService Tests (39 tests)
- ✅ Getting pitch from starting pitch and interval
- ✅ Calculating intervals between pitches
- ✅ Transposing pitches by semitones
- ✅ Handling accidentals and pitch wrapping
- ✅ Interval quality determination
- ✅ Octave interval handling

### IntervalService Tests (38 tests)
- ✅ Creating intervals from semitone and pitch offsets
- ✅ Handling negative input values (preserved as-is)
- ✅ Getting interval inverses
- ✅ Adding intervals together
- ✅ Interval quality determination for all interval types
- ✅ Normalization of large offsets
- ✅ Diminished interval handling

### ScaleService Tests (19 tests)
- ✅ Getting scale pitches from root pitch and intervals
- ✅ All repository operations (CRUD operations)
- ✅ Scale validation (names, intervals, metadata)
- ✅ Error handling for invalid scales
- ✅ Dependency injection with mocked services

### DatabaseSeeder Tests (14 tests)
- ✅ Seeding empty database
- ✅ Skipping seed when data already exists
- ✅ Database creation validation
- ✅ Error handling and logging
- ✅ Data persistence verification
- ✅ Idempotent seeding behavior

## Testing Technologies Used

- **xUnit** - Testing framework
- **FluentAssertions** - Fluent assertion library for readable tests
- **Moq** - Mocking framework for dependency isolation
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing

## Key Test Features

1. **Comprehensive Coverage**: Tests cover all public methods in service classes
2. **Edge Case Testing**: Includes tests for boundary conditions, invalid inputs, and error scenarios
3. **Isolation**: Service tests use mocks to isolate units under test
4. **Data Validation**: Tests verify both successful operations and validation failures
5. **Async Testing**: All async operations are properly tested
6. **Integration Testing**: DatabaseSeeder tests use actual EF Core context
7. **Automated Test Runner**: PowerShell script (`RunUnitTests.ps1`) handles everything from test execution to coverage reporting

## Running Tests

### Quick Start (PowerShell Script)
The easiest way to run tests with coverage reporting:

```powershell
# Run tests with coverage report (opens HTML report automatically)
.\RunUnitTests.ps1

# Run tests only (no coverage)
.\RunUnitTests.ps1 -GenerateCoverage $false

# Run tests with coverage but don't open report
.\RunUnitTests.ps1 -OpenReport $false
```

### Manual Test Execution
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "PitchServiceTests"

# Run with verbose output
dotnet test --verbosity normal
```

## Code Coverage

To generate and view code coverage reports:

### Prerequisites
The test project already includes `coverlet.collector` for coverage collection. You only need to install the report generator:

```bash
# Install ReportGenerator for HTML reports (global tool)
dotnet tool install -g dotnet-reportgenerator-globaltool
```

### Generate Coverage Reports

```bash
# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML coverage report
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html

# Open the coverage report
start TestResults/CoverageReport/index.html
```

### Coverage Metrics
The test suite provides excellent coverage across all service classes:
- **PitchService**: Comprehensive coverage of all public methods
- **IntervalService**: Full coverage including edge cases and error conditions
- **ScaleService**: Complete coverage with mocked dependencies
- **DatabaseSeeder**: Integration testing with actual database operations

### Alternative Coverage Tools
You can also use other coverage tools:

```bash
# Using dotnet-coverage (alternative)
dotnet tool install -g dotnet-coverage
dotnet-coverage collect "dotnet test" -f xml -o coverage.xml

# Using Visual Studio Code Coverage extension
# Install "Coverage Gutters" extension for VS Code to view inline coverage
```

## Test Results

All 110 tests pass successfully, providing confidence in the reliability of the service layer implementations.

## Notes

- Intervals are stored as JSON in the database, not as navigation properties
- The IntervalService preserves original offset values while using normalized values for calculations
- Scale validation ensures proper metadata and interval requirements
- Tests demonstrate proper async/await patterns throughout the codebase