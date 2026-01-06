using MembershipSystemAPI.Data;
using MembershipSystemAPI.Domain.Services;
using MembershipSystemAPI.Repositories;
using MembershipSystemAPI.Services;

namespace MembershipSystemAPI.Configuration;

public static class ServiceRegistrations
{
    public static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName));
        services.Configure<RateLimitSettings>(configuration.GetSection(RateLimitSettings.SectionName));
        services.Configure<SecuritySettings>(configuration.GetSection(SecuritySettings.SectionName));
        services.Configure<MembershipSettings>(configuration.GetSection(MembershipSettings.SectionName));

        services.AddSingleton<IJwtSettings>(sp =>
            sp.GetRequiredService<IOptions<JwtSettings>>().Value);
        services.AddSingleton<IDatabaseSettings>(sp =>
            sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);
        services.AddSingleton<IRateLimitSettings>(sp =>
            sp.GetRequiredService<IOptions<RateLimitSettings>>().Value);
        services.AddSingleton<ISecuritySettings>(sp =>
            sp.GetRequiredService<IOptions<SecuritySettings>>().Value);
        services.AddSingleton<IMembershipSettings>(sp =>
            sp.GetRequiredService<IOptions<MembershipSettings>>().Value);

        return services;
    }

    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseSettings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>();

        services.AddDbContext<MemDbContext>(options =>
        {
            options.UseSqlite(databaseSettings?.ConnectionString ?? configuration.GetConnectionString("SqliteConnection"));

            if (databaseSettings?.EnableSensitiveDataLogging == true)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }

    public static IServiceCollection AddCQRSServices(this IServiceCollection services)
    {
        // 注册MediatR服务
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserDomainService, UserDomainService>();
        return services;
    }

    public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMembershipCardRepository, MembershipCardRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IPathConfigurationRepository, PathConfigurationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

        services.AddAuthenticationJwtBearer(options =>
        {
            options.SigningKey = jwtSettings?.SecretKey ?? configuration["Jwt:SecretKey"]!;
        });

        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, ApiKeyBasedUserIdProvider>();
        services.AddSingleton<IConnectionManager, InMemoryConnectionManager>();
        services.AddScoped<ICdkService, CdkService>();
        services.AddScoped<IPathService, PathService>();
        services.AddScoped<IUserConnectionService, UserConnectionService>();
        services.AddHostedService<ExpiredMembershipProcessor>();


        return services;
    }

    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitSettings = configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (!context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    retryAfter = TimeSpan.FromSeconds(60);
                }
                await context.HttpContext.Response.WriteAsync($"请求过于频繁，请 {retryAfter.TotalSeconds} 秒后重试。", ct);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(partitionKey: ipAddress, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitSettings?.GlobalRequestsPerMinute ?? 100,
                    Window = TimeSpan.FromMinutes(1),
                });
            });

            options.AddFixedWindowLimiter("login-policy", opt =>
            {
                opt.PermitLimit = rateLimitSettings?.LoginAttemptsPerMinute ?? 5;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = rateLimitSettings?.LoginQueueLimit ?? 2;
            });

            options.AddFixedWindowLimiter("register-policy", opt =>
            {
                opt.PermitLimit = rateLimitSettings?.RegistrationAttemptsPerHour ?? 3;
                opt.Window = TimeSpan.FromHours(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });
        });

        return services;
    }

    public static IServiceCollection AddCorsServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });

        return services;
    }
}

// Settings interfaces for better testability and IoC
public interface IJwtSettings
{
    string SecretKey { get; }
    string Issuer { get; }
    string Audience { get; }
    int ExpiryInHours { get; }
}

public interface IDatabaseSettings
{
    string ConnectionString { get; }
    bool EnableSensitiveDataLogging { get; }
}

public interface IRateLimitSettings
{
    int LoginAttemptsPerMinute { get; }
    int LoginQueueLimit { get; }
    int RegistrationAttemptsPerHour { get; }
    int GlobalRequestsPerMinute { get; }
}

public interface ISecuritySettings
{
    bool RequireConfirmedAccount { get; }
    int MaxFailedAccessAttempts { get; }
    int LockoutTimeInMinutes { get; }
    bool EnableTwoFactor { get; }
}

public interface IMembershipSettings
{
    bool AutoProcessExpiredMemberships { get; }
    int ExpiredMembershipsBatchSize { get; }
    int ExpiredMembershipsProcessingIntervalMinutes { get; }
}