using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Discriminator;

public class TransformationsContext(DbContextOptions<TransformationsContext> options) : DbContext(options)
{
    public DbSet<TransformationBase> Transformations { get; set; }
    
    public DbSet<Transformation<DivisionTransformationProperties>> Divide { get; set; }
    
    public DbSet<Transformation<MultiplyTransformationProperties>> Multiply { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<TransformationBase>()
            .ToTable("transformations");

        modelBuilder.Entity<TransformationBase>()
            .HasKey(x => x.Name);
        
        modelBuilder.Entity<TransformationBase>()
            .Property(x => x.Name)
            .HasColumnName("name");

        modelBuilder
            .Entity<TransformationBase>()
            .Property(x => x.TransformationType)
            .HasColumnName("transformationtype");

        modelBuilder.Entity<TransformationBase>()
            .HasDiscriminator(x => x.TransformationType)
            .HasValue<Transformation<DivisionTransformationProperties>>(TransformationType.Divide)
            .HasValue<Transformation<MultiplyTransformationProperties>>(TransformationType.Multiply);
        
        modelBuilder.Entity<Transformation<MultiplyTransformationProperties>>()
            .Property(x => x.Properties)
            .HasColumnName("properties")
            .HasConversion<string>(
                x => JsonSerializer.Serialize(x, JsonSerializerOptions.Default),
                x => JsonSerializer.Deserialize<MultiplyTransformationProperties>(x, JsonSerializerOptions.Default));
        
        modelBuilder.Entity<Transformation<DivisionTransformationProperties>>()
            .Property(x => x.Properties)
            .HasColumnName("properties")
            .HasConversion<string>(
                x => JsonSerializer.Serialize(x, JsonSerializerOptions.Default),
                x => JsonSerializer.Deserialize<DivisionTransformationProperties>(x, JsonSerializerOptions.Default));
    }
}