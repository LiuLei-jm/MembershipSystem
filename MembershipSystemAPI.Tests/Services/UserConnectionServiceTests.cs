using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Hubs;
using MembershipSystemAPI.Repositories;
using MembershipSystemAPI.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace MembershipSystemAPI.Tests.Services;

public class UserConnectionServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPathService> _mockPathService;
    private readonly Mock<ILogger<UserConnectionService>> _mockLogger;
    private readonly UserConnectionService _userConnectionService;

    public UserConnectionServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPathService = new Mock<IPathService>();
        _mockLogger = new Mock<ILogger<UserConnectionService>>();

        _userConnectionService = new UserConnectionService(
            _mockUserRepository.Object,
            _mockPathService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ValidateApiKeyAndGetUserAsync_WithValidApiKey_ReturnsUser()
    {
        // Arrange
        var apiKey = "valid-api-key";
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", IsActive = true };

        _mockUserRepository.Setup(x => x.GetByIdWithDetailsAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userConnectionService.ValidateApiKeyAndGetUserAsync(apiKey);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Username, result.Username);
    }

    [Fact]
    public async Task ValidateApiKeyAndGetUserAsync_WithInvalidApiKey_ReturnsNull()
    {
        // Arrange
        var apiKey = "invalid-api-key";

        _mockUserRepository.Setup(x => x.GetByIdWithDetailsAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userConnectionService.ValidateApiKeyAndGetUserAsync(apiKey);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateApiKeyAndGetUserAsync_WithNullApiKey_ReturnsNull()
    {
        // Act
        var result = await _userConnectionService.ValidateApiKeyAndGetUserAsync(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateApiKeyAndGetUserAsync_WithEmptyApiKey_ReturnsNull()
    {
        // Act
        var result = await _userConnectionService.ValidateApiKeyAndGetUserAsync(string.Empty);

        // Assert
        Assert.Null(result);
    }
}