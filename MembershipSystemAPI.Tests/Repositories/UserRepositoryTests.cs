using MembershipSystemAPI.Data;
using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MembershipSystemAPI.Tests.Repositories;

public class UserRepositoryTests
{
    private readonly Mock<MemDbContext> _mockDbContext;
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        _mockDbContext = new Mock<MemDbContext>();
        _userRepository = new UserRepository(_mockDbContext.Object);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUsername_ReturnsUser()
    {
        // Arrange
        var username = "testuser";
        var user = new User { Id = Guid.NewGuid(), Username = username };

        var mockDbSet = new Mock<DbSet<User>>();
        mockDbSet.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockDbContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _userRepository.GetByUsernameAsync(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistingUsername_ReturnsNull()
    {
        // Arrange
        var username = "nonexistentuser";

        var mockDbSet = new Mock<DbSet<User>>();
        mockDbSet.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockDbContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _userRepository.GetByUsernameAsync(username);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_WithValidId_ReturnsUserWithDetails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            ApiKey = new ApiKey { Key = "test-api-key" },
            PathConfiguration = new PathConfiguration { BasePath = "D:" }
        };

        var mockDbSet = new Mock<DbSet<User>>();
        mockDbSet.Setup(x => x.Include(It.IsAny<string>()))
            .Returns(mockDbSet.Object);
        mockDbSet.Setup(x => x.FirstOrDefaultAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockDbContext.Setup(x => x.Users).Returns(mockDbSet.Object);

        // Act
        var result = await _userRepository.GetByIdWithDetailsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.NotNull(result.ApiKey);
        Assert.NotNull(result.PathConfiguration);
    }
}