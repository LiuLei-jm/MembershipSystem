using MembershipSystemAPI.Domain.Entities;
using MembershipSystemAPI.DTOs;
using MembershipSystemAPI.Hubs;
using MembershipSystemAPI.Repositories;
using MembershipSystemAPI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace MembershipSystemAPI.Tests.Services;

public class ExpiredMembershipProcessorTests
{
    private readonly Mock<ILogger<ExpiredMembershipProcessor>> _mockLogger;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IMembershipCardRepository> _mockMembershipCardRepository;
    private readonly Mock<IHubContext<FilePushHub>> _mockHubContext;
    private readonly Mock<IPathService> _mockPathService;
    private readonly Mock<IConnectionManager> _mockConnectionManager;
    private readonly ExpiredMembershipProcessor _expiredMembershipProcessor;

    public ExpiredMembershipProcessorTests()
    {
        _mockLogger = new Mock<ILogger<ExpiredMembershipProcessor>>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockMembershipCardRepository = new Mock<IMembershipCardRepository>();
        _mockHubContext = new Mock<IHubContext<FilePushHub>>();
        _mockPathService = new Mock<IPathService>();
        _mockConnectionManager = new Mock<IConnectionManager>();

        // Setup service provider to return mocked services
        _mockServiceProvider.Setup(x => x.GetService(typeof(IMembershipCardRepository)))
            .Returns(_mockMembershipCardRepository.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IHubContext<FilePushHub>)))
            .Returns(_mockHubContext.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IPathService)))
            .Returns(_mockPathService.Object);
        _mockServiceProvider.Setup(x => x.GetService(typeof(IConnectionManager)))
            .Returns(_mockConnectionManager.Object);

        _expiredMembershipProcessor = new ExpiredMembershipProcessor(
            _mockLogger.Object,
            _mockServiceProvider.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldStartTimer()
    {
        // Act
        await _expiredMembershipProcessor.StartAsync(CancellationToken.None);

        // Assert
        // Timer should be created and started
        // We can't directly assert on private _timer field, but we can verify logger was called
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("会员卡过期处理服务启动")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void StopAsync_ShouldStopTimer()
    {
        // Arrange
        _expiredMembershipProcessor.StartAsync(CancellationToken.None).Wait();

        // Act
        var result = _expiredMembershipProcessor.StopAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("会员卡过期处理服务停止")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}