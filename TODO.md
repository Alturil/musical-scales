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
- **Should Fix**: Consider DTOs for API endpoints
- **Consider**: JSON storage strategy if querying intervals becomes important
- **Acceptable for Demo**: Blocking startup seeding

## Completed

- ✅ **Remove Dead Code in DbContext** - Removed unused `SeedData` method and its call from `MusicalScalesDbContext.cs`
- ✅ **Use camelCase JSON Convention** - Changed `PropertyNamingPolicy` from `null` to `JsonNamingPolicy.CamelCase` in `Program.cs`

## Notes

For a learning/demo project showcasing music theory OOP, current tradeoffs are reasonable. For production deployment, prioritize:
1. Add DTOs
2. Reconsider JSON storage strategy if interval-based queries become important
