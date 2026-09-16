using AuthApi.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace AuthApi.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
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
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // نحدد الـ Status Code حسب نوع الخطأ
        var statusCode = exception switch
        {
            AppException appEx => appEx.StatusCode,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
            KeyNotFoundException => (int)HttpStatusCode.NotFound,            // 404
            ArgumentException => (int)HttpStatusCode.BadRequest,             // 400
            _ => (int)HttpStatusCode.InternalServerError                     // 500
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            success = false,
            message = exception.Message,
            // في التطوير فقط نعرض الـ StackTrace
            details = _env.IsDevelopment() ? exception.StackTrace : null
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}