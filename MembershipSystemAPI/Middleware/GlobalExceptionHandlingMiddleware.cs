using System.Net;
using System.Text.Json;

namespace MembershipSystemAPI.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case ArgumentNullException ex:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = ex.Message;
                errorResponse.Details = "参数为空";
                break;

            case ArgumentException ex:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = ex.Message;
                errorResponse.Details = "参数错误";
                break;

            case UnauthorizedAccessException ex:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "未授权访问";
                errorResponse.Details = ex.Message;
                break;

            case KeyNotFoundException ex:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "找不到请求的资源";
                errorResponse.Details = ex.Message;
                break;

            case InvalidOperationException ex:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "无效的操作";
                errorResponse.Details = ex.Message;
                break;

            case TimeoutException ex:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse.Message = "请求超时";
                errorResponse.Details = ex.Message;
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                // 在生产环境中不暴露详细的错误信息
                if (_environment.IsDevelopment())
                {
                    errorResponse.Message = exception.Message;
                    errorResponse.Details = exception.StackTrace ?? "服务器内部错误";
                }
                else
                {
                    errorResponse.Message = "Internal Server Error";
                    errorResponse.Details = "服务器内部错误，请联系技术支持";
                }
                break;
        }

        errorResponse.StatusCode = response.StatusCode;
        errorResponse.Timestamp = DateTime.UtcNow;

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}