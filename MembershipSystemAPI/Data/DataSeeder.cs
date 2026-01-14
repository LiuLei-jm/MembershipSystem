

using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.Domain.Enums;

namespace MembershipSystemAPI.Data;

public static class DataSeeder
{
    public static async Task SeedDefaultAdminUserAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var dbContext = services.GetRequiredService<MemDbContext>();
            var logger = services.GetRequiredService<ILogger<Program>>();
            const string defaultAdminUsername = "MemAdmin";
            const string defaultAdminPassword = "MemAdmin2025";
            if (!await dbContext.Users.AnyAsync(u => u.Username == defaultAdminUsername))
            {
                logger.LogInformation("没有找到默认管理员账户，开始创建...");
                var Id = Guid.NewGuid();
                var defaultAdminPasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultAdminPassword);
                var adminUser = User.Create(defaultAdminUsername, defaultAdminPasswordHash, UserRole.Admin);

                await dbContext.Users.AddAsync(adminUser);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("默认管理员已创建: {Username} 密码: {Password}", defaultAdminUsername, defaultAdminPassword);
            }
            else
            {
                logger.LogInformation("默认管理员已经存在.");
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "为数据库设定种子时发生错误.");
        }
    }
}
