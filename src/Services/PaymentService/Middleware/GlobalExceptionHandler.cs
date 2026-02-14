using System.Net;
using System.Text.Json;
using Shared.Exceptions;
using Shared.Models;

namespace PaymentService.Middleware;

/// <summary>
/// Global exception handler middleware for consistent error responses
/// </summary>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, exception.Message),
            BadRequestException => (HttpStatusCode.BadRequest, exception.Message),
            PaymentFailedException => (HttpStatusCode.PaymentRequired, exception.Message),
            _ => (HttpStatusCode.InternalServerError, _env.IsDevelopment()
                ? exception.Message
                : "An internal server error occurred")
        };

        context.Response.StatusCode = (int)statusCode;

        var errors = new List<string> { exception.Message };

        // Include stack trace only in development
        if (_env.IsDevelopment() && exception.StackTrace != null)
        {
            errors.Add(exception.StackTrace);
        }

        var response = ApiResponse<object>.FailureResponse(
            message,
            (int)statusCode,
            errors
        );

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension method to register the global exception handler
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandler>();
    }
}
