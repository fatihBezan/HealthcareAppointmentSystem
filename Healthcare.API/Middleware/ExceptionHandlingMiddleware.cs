using Healthcare.Application.Exceptions;
using System.Net;
using System.Text.Json;
using AppUnauthorizedAccessException = Healthcare.Application.Exceptions.UnauthorizedAccessException;
using SystemUnauthorizedAccessException = System.UnauthorizedAccessException;

namespace Healthcare.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "An error occurred while processing your request.";

            switch (exception)
            {
                case DoctorLimitExceededException doctorLimitEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = doctorLimitEx.Message;
                    break;
                case AppointmentLimitExceededException appointmentLimitEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = appointmentLimitEx.Message;
                    break;
                case AppUnauthorizedAccessException unauthorizedEx:
                    statusCode = HttpStatusCode.Forbidden;
                    message = unauthorizedEx.Message;
                    break;
                case SystemUnauthorizedAccessException systemUnauthorizedEx:
                    statusCode = HttpStatusCode.Forbidden;
                    message = systemUnauthorizedEx.Message;
                    break;
                case AppException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = exception.Message;
                    break;
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = exception.Message;
                    break;
                default:
                    // Log the detailed error for internal exceptions
                    _logger.LogError(exception, "Unhandled exception occurred");
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = (int)statusCode,
                message = message
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }

    // Extension method to add the middleware to the HTTP request pipeline
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
} 