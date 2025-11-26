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
    }
}