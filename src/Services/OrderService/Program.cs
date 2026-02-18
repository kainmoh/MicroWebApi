using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Middleware;
using OrderService.Services;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/order-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Order Service API",
        Version = "v1",
        Description = "Microservice for managing orders with Saga orchestration pattern"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Database Configuration
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    )
);

// Register Saga Orchestrator
builder.Services.AddScoped<IOrderSagaOrchestrator, OrderSagaOrchestrator>();

// ====================================================================
// Circuit Breaker Pattern with Polly
// ====================================================================
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (outcome, timespan) =>
        {
            Log.Warning("Circuit breaker opened for {Duration}s due to: {Exception}",
                timespan.TotalSeconds, outcome.Exception?.Message ?? "Policy result");
        },
        onReset: () =>
        {
            Log.Information("Circuit breaker reset");
        },
        onHalfOpen: () =>
        {
            Log.Information("Circuit breaker is half-open");
        }
    );

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Log.Warning("Retry {RetryCount} after {Delay}s due to: {Exception}",
                retryCount, timespan.TotalSeconds, outcome.Exception?.Message ?? "Policy result");
        }
    );

// Configure HTTP clients with Circuit Breaker and Retry policies
builder.Services.AddHttpClient("ProductService")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy)
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

builder.Services.AddHttpClient("PaymentService")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy)
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseGlobalExceptionHandler();

// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API V1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Database Migration
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        Log.Information("Applying database migrations for Order Service...");
        dbContext.Database.Migrate();
        Log.Information("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while migrating the database");
        throw;
    }
}

Log.Information("Starting Order Service on {Url}", builder.Configuration["Urls"]);
app.Run();

// Make the implicit Program class public for testing
public partial class Program { }
