namespace MembershipSystemAPI.Middleware;

/// <summary>
/// 中间件用于添加安全相关的HTTP头
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 添加安全头
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "no-referrer");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        await _next(context);
    }
}

/// <summary>
/// SecurityHeadersMiddleware的扩展方法
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// 添加安全头中间件
    /// </summary>
    /// <param name="builder">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}