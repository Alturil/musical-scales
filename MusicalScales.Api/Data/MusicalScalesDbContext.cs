using Microsoft.EntityFrameworkCore;
using MusicalScales.Api.Models;
using System.Text.Json;

namespace MusicalScales.Api.Data;

/// <summary>
/// Database context for the Musical Scales API
/// </summary>
public class MusicalScalesDbContext : DbContext
{
    public MusicalScalesDbContext(DbContextOptions<MusicalScalesDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// Scales table
    /// </summary>
    public DbSet<Scale> Scales { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Scale entity
        modelBuilder.Entity<Scale>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // Configure complex properties as JSON
            entity.OwnsOne(e => e.Metadata, metadata =>
            {
                var namesProperty = metadata.Property(m => m.Names)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                    )
                    .HasColumnName("MetadataNames");
                
                namesProperty.Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IList<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
                    
                metadata.Property(m => m.Description)
                    .HasColumnName("MetadataDescription");
                    
                metadata.Property(m => m.Origin)
                    .HasColumnName("MetadataOrigin");
            });
            
            var intervalsProperty = entity.Property(e => e.Intervals)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<Interval>>(v, (JsonSerializerOptions?)null) ?? new List<Interval>()
                );
            
            intervalsProperty.Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IList<Interval>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()));
        });
        
        // Seed some basic scales
        SeedData(modelBuilder);
    }
    
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var majorScale = new Scale
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            Metadata = new ScaleMetadata 
            { 
                Names = new List<string> { "Major Scale", "Ionian Mode" },
                Description = "The most common scale in Western music",
                Origin = "Western"
            },
            Intervals = new List<Interval>
            {
                new() { Name = Models.Enums.IntervalSizeName.Second, Quality = Models.Enums.IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                new() { Name = Models.Enums.IntervalSizeName.Third, Quality = Models.Enums.IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 },
                new() { Name = Models.Enums.IntervalSizeName.Fourth, Quality = Models.Enums.IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                new() { Name = Models.Enums.IntervalSizeName.Fifth, Quality = Models.Enums.IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                new() { Name = Models.Enums.IntervalSizeName.Sixth, Quality = Models.Enums.IntervalQualityName.Major, PitchOffset = 5, SemitoneOffset = 9 },
                new() { Name = Models.Enums.IntervalSizeName.Seventh, Quality = Models.Enums.IntervalQualityName.Major, PitchOffset = 6, SemitoneOffset = 11 },
                new() { Name = Models.Enums.IntervalSizeName.Octave, Quality = Models.Enums.IntervalQualityName.Perfect, PitchOffset = 7, SemitoneOffset = 12 }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var minorScale = new Scale
        {
            Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
            Metadata = new ScaleMetadata 
            { 
                Names = new List<string> { "Natural Minor Scale", "Aeolian Mode" },
                Description = "The natural minor scale",
                Origin = "Western"
            },
            Intervals = new List<Interval>
            {
                new() { Name = Models.Enums.IntervalSizeName.Second, Quality = Models.Enums.IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                new() { Name = Models.Enums.IntervalSizeName.Third, Quality = Models.Enums.IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
                new() { Name = Models.Enums.IntervalSizeName.Fourth, Quality = Models.Enums.IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                new() { Name = Models.Enums.IntervalSizeName.Fifth, Quality = Models.Enums.IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                new() { Name = Models.Enums.IntervalSizeName.Sixth, Quality = Models.Enums.IntervalQualityName.Minor, PitchOffset = 5, SemitoneOffset = 8 },
                new() { Name = Models.Enums.IntervalSizeName.Seventh, Quality = Models.Enums.IntervalQualityName.Minor, PitchOffset = 6, SemitoneOffset = 10 },
                new() { Name = Models.Enums.IntervalSizeName.Octave, Quality = Models.Enums.IntervalQualityName.Perfect, PitchOffset = 7, SemitoneOffset = 12 }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // Note: EF Core seed data with complex types requires special handling
        // We'll add the seed data through the repository or a data seeder service instead
    }
}