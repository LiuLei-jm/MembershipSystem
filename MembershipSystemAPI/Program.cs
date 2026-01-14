using MembershipSystemAPI.Configuration;
using MembershipSystemAPI.Data;
using MembershipSystemAPI.Hubs;
using MembershipSystemAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 配置Serilog
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Host.UseSerilog();

// 配置设置
builder.Services.AddConfigurationSettings(builder.Configuration);

// 数据库服务
builder.Services.AddDatabaseServices(builder.Configuration);

// CQRS服务
builder.Services.AddCQRSServices();

// 领域服务
builder.Services.AddDomainServices();

// 仓储服务
builder.Services.AddRepositoryServices();

// 应用服务
builder.Services.AddApplicationServices(builder.Configuration);

// 限流服务
builder.Services.AddRateLimitingServices(builder.Configuration);

// 基础服务
builder.Services.AddFastEndpoints().SwaggerDocument();

// CORS服务
builder.Services.AddCorsServices();

var app = builder.Build();

// 数据库迁移
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MemDbContext>();
    db.Database.Migrate();
<<<<<<< HEAD
    await app.SeedDefaultAdminUserAsync();
    // 种子数据
=======
    // 种子数据
    await app.SeedDefaultAdminUserAsync();
>>>>>>> main
}
else
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MemDbContext>();

    try
    {
        db.Database.Migrate();
<<<<<<< HEAD
        await app.SeedDefaultAdminUserAsync();
    }
    catch (Exception ex)
    {
        // In production, ensure proper environment variables are set for database connection
        // Database__ConnectionString or ConnectionStrings__SqliteConnection should be set as environment variables
=======
    }
    catch (Exception ex)
    {
>>>>>>> main
        Directory.CreateDirectory(@"D:\Logs\MembershipAPI");
        File.WriteAllText(@"D:\Logs\MembershipAPI\db-migration-error.txt", ex.ToString());
        throw;
    }
}

// 中间件配置
app.UseGlobalExceptionHandling();
app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// 端点配置
if (app.Environment.IsDevelopment())
{
    app.UseFastEndpoints().UseSwaggerGen();
}
else
{
    app.UseFastEndpoints();
}

app.UseCors();
app.MapHub<FilePushHub>("/filePushHub");

app.Run();
