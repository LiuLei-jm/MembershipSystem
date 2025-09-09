using Microsoft.AspNetCore.SignalR;

namespace MembershipSystemAPI.Hubs
{
    public class CommandHub : Hub
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CommandHub> _logger;
        public CommandHub(IConfiguration configuration, ILogger<CommandHub> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            var providedApiKey = Context.GetHttpContext().Request.Query["apiKey"].ToString();
            var serverApiKey = _configuration["ApiKey"];

            if (string.IsNullOrEmpty(providedApiKey) || providedApiKey != serverApiKey)
            {
                _logger.LogWarning("未经授权尝试连接 CommandHub。.");
                Context.Abort();
                return Task.CompletedTask;
            }
            _logger.LogInformation($"客户端连接成功：{Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"客户端断开连接：{Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
