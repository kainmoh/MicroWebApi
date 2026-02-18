using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Shared.Models;

namespace UserService.Middleware;

/// <summary>
/// Global exception handler middleware for User Service
/// </summary>
public static class GlobalExceptionHandler
{
    public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionHandlerFeature?.Error;

                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred processing your request",
                    Data = null
                };

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                if (exception != null)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(exception, "Unhandled exception occurred");
                    response.Message = exception.Message;
                }

                await context.Response.WriteAsJsonAsync(response);
            });
        });
    }
}
