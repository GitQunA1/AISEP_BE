using System.Net;
using System.Text.Json;
using AISEP.BLL.Exceptions;
using AISEP.BLL.Helpers;
using FluentValidation;

namespace AISEP.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, title) = ex switch
            {
                ValidationException => ((int)HttpStatusCode.BadRequest, "Validation failed"),
                ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad request"),
                KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Not found"),
                ForbiddenAccessException => ((int)HttpStatusCode.Forbidden, "Forbidden"),
                UnauthorizedAccessException => ((int)HttpStatusCode.Forbidden, "Forbidden"),
                InvalidOperationException => ((int)HttpStatusCode.Conflict, "Conflict"),
                HttpRequestException => (502, "Upstream service error"),
                _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            if (statusCode >= 500)
            {
                _logger.LogError(ex, "Unhandled exception at {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            else
            {
                _logger.LogWarning(ex, "Handled exception at {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var errorMessage = statusCode == (int)HttpStatusCode.InternalServerError
                ? "An unexpected error occurred."
                : ex.Message;

            var payload = ApiResponse<object>.ErrorResponse(errorMessage, title, statusCode);
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
