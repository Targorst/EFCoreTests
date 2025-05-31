using System.Reflection;
using System.Text.Json.Serialization;
using Discriminator;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddMapster();
builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssembly(Assembly.GetEntryAssembly());
    x.RegisterGenericHandlers = true;
});
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
app.MapPost("/transformation", async (
        TransformationsContext context,
        IMediator mediator,
        [FromBody] CreateTransformationDto createTransformation) =>
    {
        var command = CreateGenericTypeInstance(createTransformation.Properties);

        static object CreateGenericTypeInstance(ITransformationProperties properties)
        {
            Type[] typeArgs = [properties.GetType()];
            var type = typeof(CreateTransformationGenericCommand<>).MakeGenericType(typeArgs);

            var transformationCommand = Activator.CreateInstance(type, args: [properties]);
            return transformationCommand ?? throw new InvalidOperationException($"Could not create command of type {type}");
        }
        
        await mediator.Send(command);
        
        // context.Transformations.Add(transformation);
        // context.SaveChanges();
        
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

public record CreateTransformationDto
{
    public ITransformationProperties Properties { get; init; }
}

public record CreateTransformationGenericCommand<T>(T Properties) : IRequest where T : class, ITransformationProperties;

public record CreateTransformationGenericCommandHandler<T>
    : IRequestHandler<CreateTransformationGenericCommand<T>> where T : class, ITransformationProperties
{
    private readonly TransformationsContext _context;
    
    public CreateTransformationGenericCommandHandler(TransformationsContext context) => _context = context;
    
    public async Task Handle(CreateTransformationGenericCommand<T> request, CancellationToken ct)
    {
        var transformation = new Transformation<T>(request.Properties, "456");
        _context.Transformations.Add(transformation);
        await _context.SaveChangesAsync(ct);
    }
}