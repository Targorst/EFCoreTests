using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TransformationsContext>(options =>
{
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
    options.UseNpgsql("Host=localhost; Database=postgres; Username=***; Password=***");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// !!! В ТЕЛЕ POST Запроса первым параметров всегда должен быть тот параметр,
// !!! на основе которого резолвится зависимость.
app.MapPost("/transformation", (TransformationsContext context, [FromBody] ITransformationProperties properties) =>
    {
        var transformation = new Transformation<DivisionTransformationProperties>((DivisionTransformationProperties)properties, "Альфа");
        
        context.Transformations.Add(transformation);
        context.SaveChanges();
        
        var result1 = context.Multiply
            .ToList();
        
        var result2 = context.Divide
            .ToList();
        
        var result3 = context.Transformations
            .ToList();
        
        int l = 1;
    })
    .WithName("CreateTransformation")
    .WithOpenApi();

app.Run();

public abstract class TransformationBase
{
    public TransformationType TransformationType { get; init; }

    public string Name { get; init; }
}

public class Transformation<T> : TransformationBase where T : ITransformationProperties
{
    public Transformation(T properties, string name)
    {
        Properties = properties;
        Name = name;
    }
    
    public T Properties { get; init; }
}

public enum TransformationType
{
    Divide,
    Multiply
}

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

[JsonPolymorphic(TypeDiscriminatorPropertyName = "TransformationType")]
[JsonDerivedType(typeof(DivisionTransformationProperties), typeDiscriminator: "Divide")]
[JsonDerivedType(typeof(MultiplyTransformationProperties), typeDiscriminator: "Multiply")]
public interface ITransformationProperties
{
    public TransformationType TransformationType { get; }
}

public class DivisionTransformationProperties : ITransformationProperties
{
    public TransformationType TransformationType => TransformationType.Divide;

    public int DivideBy { get; init; }

    public int NumberOne { get; init; }
    
    public int NumberTwo { get; init; }
}

public class MultiplyTransformationProperties : ITransformationProperties
{
    public TransformationType TransformationType => TransformationType.Multiply;
    
    public int MultiplyOn { get; init; }

    public int FirstNumber { get; init; }
    
    public int SecondNumber { get; init; }
}