using MembershipSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MembershipSystemAPI.Services;

public interface IPathService
{
    Task<PathConfiguration> GetUserPathConfigurationAsync(Guid userId);
    Task<PathConfiguration> UpdateUserPathConfigurationAsync(Guid userId, PathConfigurationUpdateRequest request);
    string GetFullFilePath(Guid userId, string fileName);
    bool IsPathValid(string path);
}

public class PathService : IPathService
{
    private readonly MemDbContext _dbContext;
    private readonly ILogger<PathService> _logger;

    public PathService(MemDbContext dbContext, ILogger<PathService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<PathConfiguration> GetUserPathConfigurationAsync(Guid userId)
    {
        var user = await _dbContext.Users
            .Include(u => u.PathConfiguration)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new ArgumentException("用户没有找到", nameof(userId));
        }

        return user.PathConfiguration;
    }

    public async Task<PathConfiguration> UpdateUserPathConfigurationAsync(Guid userId, PathConfigurationUpdateRequest request)
    {
        // Validate request
        request.Validate();

        var user = await _dbContext.Users
            .Include(u => u.PathConfiguration)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new ArgumentException("用户没有找到", nameof(userId));
        }

        // Validate paths
        if (!IsPathValid(request.BasePath))
        {
            throw new ArgumentException("无效的基本路径", nameof(request.BasePath));
        }

        // Update path configuration
        user.PathConfiguration.BasePath = request.BasePath;
        user.PathConfiguration.MembershipCardFilePath = request.MembershipCardFilePath;
        user.PathConfiguration.AllowCustomPaths = request.AllowCustomPaths;
        user.PathConfiguration.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return user.PathConfiguration;
    }

    public string GetFullFilePath(Guid userId, string fileName)
    {
        // Fetch user with path configuration
        var user = _dbContext.Users
            .Include(u => u.PathConfiguration)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            // Return default path if user not found
            return Path.Combine("D:", fileName);
        }

        // Validate file name to prevent path traversal
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("文件名不能为空.", nameof(fileName));
        }

        // Ensure fileName doesn't contain path traversal characters or directory separators
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\") || fileName.Contains(":"))
        {
            throw new ArgumentException("无效的文件名 - 不能包含路径分隔符或遍历字符", nameof(fileName));
        }

        // Validate that the file name doesn't exceed reasonable limits
        if (fileName.Length > 255)
        {
            throw new ArgumentException("文件名不能太长.", nameof(fileName));
        }

        // Validate that the base path is still valid
        if (!IsPathValid(user.PathConfiguration.BasePath))
        {
            _logger.LogWarning($"用户 {userId} 具有无效的基本路径: {user.PathConfiguration.BasePath}");
            // Fall back to default path
            return Path.Combine("D:", fileName);
        }

        try
        {
            // Combine base path with file name
            var fullPath = Path.Combine(user.PathConfiguration.BasePath, fileName);

            // Additional security: ensure the path is still within expected bounds
            // This prevents cases where the base path might have been tampered with
            var fullPathRoot = Path.GetPathRoot(fullPath);
            var basePathRoot = Path.GetPathRoot(user.PathConfiguration.BasePath);
            if (!string.IsNullOrEmpty(basePathRoot) && !basePathRoot.EndsWith(@"\"))
            {
                basePathRoot += @"\";
            }
            if (fullPathRoot != basePathRoot)
            {
                _logger.LogWarning($"检查到用户 {userId} 的路径遍历尝试: {fullPath}");
                throw new ArgumentException("无效的路径组合", nameof(fileName));
            }

            return fullPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"用户组合路径错误 {userId}: 基本路径={user.PathConfiguration.BasePath}, 文件名={fileName}");
            // Fall back to default path
            return Path.Combine("D:", fileName);
        }
    }

    public bool IsPathValid(string path)
    {
        // Security check: prevent path traversal attacks
        if (string.IsNullOrWhiteSpace(path))
            return false;

        // Check for invalid characters
        var invalidChars = Path.GetInvalidPathChars();
        if (path.Any(c => invalidChars.Contains(c)))
            return false;

        // Prevent path traversal attacks
        if (path.Contains(".."))
            return false;

        // Check if it's a valid Windows path format
        // Allow drive letters like "C:", "D:", etc.
        if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
        {
            // Valid drive letter format
            return true;
        }

        // Allow UNC paths
        if (path.StartsWith(@"\\"))
        {
            return true;
        }

        // For other paths, ensure they don't start with special characters
        if (path.StartsWith("/") || path.StartsWith("\\"))
        {
            return false;
        }

        return true;
    }
}

public class PathConfigurationUpdateRequest
{
    public string BasePath { get; set; } = "D:";
    public string MembershipCardFilePath { get; set; } = "CDK.txt";
    public bool AllowCustomPaths { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BasePath))
            throw new ArgumentException("Base path cannot be empty", nameof(BasePath));

        if (string.IsNullOrWhiteSpace(MembershipCardFilePath))
            throw new ArgumentException("Membership card file path cannot be empty", nameof(MembershipCardFilePath));

        // Validate file name doesn't contain invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        if (MembershipCardFilePath.Any(c => invalidChars.Contains(c)))
            throw new ArgumentException("Membership card file path contains invalid characters", nameof(MembershipCardFilePath));
    }
}