var bld = WebApplication.CreateBuilder();
bld.Services.AddFastEndpoints()
            .SwaggerDocument();
bld.Services.AddDbContext<MemDbContext>(options =>
    options.UseSqlite(bld.Configuration.GetConnectionString("SqliteConnection")));
bld.Services.AddAuthenticationJwtBearer(s => s.SigningKey = bld.Configuration["Jwt:SecretKey"]);
bld.Services.AddSignalR();
bld.Services.AddSingleton<IUserIdProvider, ApiKeyBasedUserIdProvider>();
bld.Services.AddSingleton<IConnectionManager, InMemoryConnectionManager>();
bld.Services.AddScoped<ICdkService, CdkService>();
bld.Services.AddHostedService<ExpiredMembershipProcessor>();

bld.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (!context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            retryAfter = TimeSpan.FromSeconds(60);
        }
        await context.HttpContext.Response.WriteAsync($"请求过于频繁。请在 {retryAfter.TotalSeconds} 秒后重试。", ct);
    };

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey:ipAddress, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
        });
    });

    options.AddFixedWindowLimiter("login-policy", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    options.AddFixedWindowLimiter("register-policy", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromHours(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

bld.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = bld.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MemDbContext>();
    db.Database.Migrate();
}
await app.SeedDefaultAdminUserAsync();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints()
   .UseSwaggerGen();
app.MapHub<FilePushHub>("/filePushHub");
app.Run();
