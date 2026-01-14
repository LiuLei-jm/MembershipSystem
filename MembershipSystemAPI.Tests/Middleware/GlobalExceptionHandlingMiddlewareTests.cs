using MembershipSystemAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace MembershipSystemAPI.Tests\Middleware;

public class GlobalExceptionHandlingMiddlewareTests
{
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly Mock<ILogger<GlobalExceptionHandlingMiddleware>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public GlobalExceptionHandlingMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<GlobalExceptionHandlingMiddleware>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
    }

    [Fact]
    public async Task InvokeAsync_WithNoException_CallsNext()
    {
        // Arrange
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        var middleware = new GlobalExceptionHandlingMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithException_LogsErrorAndHandlesException()
    {
        // Arrange
        var exception = new Exception("Test exception");
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
            .Throws(exception);

        var middleware = new GlobalExceptionHandlingMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockEnvironment.Object);

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}