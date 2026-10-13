using System.Text.Json;
using System.Text.Json.Serialization;
using AppTodoList.Api.Data;
using AppTodoList.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AppTodoList");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta configurar ConnectionStrings:AppTodoList para el entorno actual.");
}

var origenesPermitidos = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

if (builder.Environment.IsProduction() && origenesPermitidos.Length == 0)
{
    throw new InvalidOperationException(
        "Production requiere configurar Cors:AllowedOrigins con los orígenes web aprobados.");
}

builder.Services.AddDbContext<TareasDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<ITareaServicio, ServicioTareas>();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("sqlite");
builder.Services.AddCors(options =>
{
    options.AddPolicy("web", policy =>
    {
        if (origenesPermitidos.Length > 0)
        {
            policy.WithOrigins(origenesPermitidos);
        }

        policy.AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false));
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<TareasDbContext>();
    await database.Database.MigrateAsync();
}

app.Use(async (context, siguiente) =>
{
    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("AppTodoList.Api.Http");
    using (logger.BeginScope(new Dictionary<string, object?>
    {
        ["RequestId"] = context.TraceIdentifier
    }))
    {
        try
        {
            await siguiente();
        }
        finally
        {
            logger.LogInformation(
                "HTTP {Method} {Path} respondió con {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode);
        }
    }
});

app.UseExceptionHandler();

if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("web");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program
{
}
