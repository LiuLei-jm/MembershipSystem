using MembershipSystemWPF.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using Serilog.Core;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MembershipSystemWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HubConnection _hubConnection = null!;
        private const string _configFileName = "config.json";
        private Logger _fileLogger = null!;
        private CancellationTokenSource _connectionCts = new();
        public MainWindow()
        {
            InitializeComponent();
            InitializeLogger();
            //CleanupOldLogs();
        }

        private void CleanupOldLogs()
        {
            Log("正在清理旧日志文件...");
            try
            {
                string logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Log("日志目录不存在，无需清理旧日志文件。");
                    return;
                }
                var files = Directory.GetFiles(logDirectory, "log-*.txt");
                int deletedCount = 0;
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < DateTime.Now.AddDays(-30))
                    {
                        fileInfo.Delete();
                        deletedCount++;
                    }
                }
                if (deletedCount > 0)
                {
                    Log($"成功删除了 {deletedCount} 个过期的日志文件。");
                }
                else
                {
                    Log("没有找到需要删除的过期日志文件。");
                }
            }
            catch (Exception ex)
            {
                Log($"清理旧日志文件时发生错误: {ex.Message}");
            }
        }

        private void InitializeLogger()
        {
            string logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
            _fileLogger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(path: Path.Combine(logDirectory, "log-.txt"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30
                , outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();
            Log("日志系统已初始化。");
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadConfigAndConnectAsync();
            if (!string.IsNullOrEmpty(ServerUrlTextBox.Text) && !string.IsNullOrEmpty(ApiKeyTextBox.Text))
            {
                Log("配置加载成功 ，开始自动连接...");
                await StartConnectionProcess();
            }
        }

        private async Task LoadConfigAndConnectAsync()
        {
            string configFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configFileName);
            Log($"正在从{configFilePath}加载配置...");
            if (!File.Exists(configFilePath))
            {
                Log($"配置文件不存在: {configFilePath}");
                return;
            }
            try
            {
                string jsonString = File.ReadAllText(configFilePath);
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    Log("配置文件内容为空。");
                    return;
                }
                var config = JsonSerializer.Deserialize<ConnectionConfig>(jsonString);
                ServerUrlTextBox.Text = config?.ServerUrl ?? string.Empty;
                ApiKeyTextBox.Text = config?.ApiKey ?? string.Empty;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log($"加载配置文件时发生错误: {ex.Message}");
                return;
            }
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_connectionCts != null)
            {
                await StopConnectionProcess();
            }
            else
            {
                await StartConnectionProcess();
            }
        }
        private async Task StartConnectionProcess()
        {
            if (_connectionCts != null)
            {
                await StopConnectionProcess();
            }
            _connectionCts = new CancellationTokenSource();


            ConnectButton.Content = "断开连接";
            ConnectButton.IsEnabled = true;
            ServerUrlTextBox.IsEnabled = false;
            ApiKeyTextBox.IsEnabled = false;
            SaveButton.IsEnabled = false;

            _ = ConnectInLoopAsync(_connectionCts.Token);
        }
        private async Task StopConnectionProcess()
        {
            if (_connectionCts == null) return;
            Log("用户请求断开连接...");
            _connectionCts.Cancel();
            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                await _hubConnection.StopAsync();
            }
            _connectionCts = null!;

            ConnectButton.Content = "连接";
            ConnectButton.IsEnabled = true;
            ServerUrlTextBox.IsEnabled = true;
            ApiKeyTextBox.IsEnabled = true;
            SaveButton.IsEnabled = true;
            Log("连接已手动断开.");
        }

        private async Task ConnectInLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string serverUrl = Dispatcher.Invoke(() => ServerUrlTextBox.Text);
                    string apiKey = Dispatcher.Invoke(() => ApiKeyTextBox.Text);
                    string serverUrlFull = $"http://{ServerUrlTextBox.Text}/commandhub";
                    string urlWithKey = $"{serverUrlFull}?apiKey={Uri.EscapeDataString(apiKey)}";

                    _hubConnection = new HubConnectionBuilder()
                        .WithUrl(urlWithKey)
                        .Build();

                    _hubConnection.Closed += (error) =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            Log($"与服务器的连接意外断开: {error?.Message}");
                        }

                        return Task.CompletedTask;
                    };

                    _hubConnection.On<FileWriteCommand>("ReceiveWriteCommand", async (command) =>
                    {
                        await ModifyFile_Append(command.FilePath, command.Content, command.LogMessage);
                    });

                    _hubConnection.On<FileDeleteCommand>("ReceiveDeleteCommand", async (command) =>
                    {
                        await RemoveContentFromFile(command.FilePath, command.ContentToRemove, command.LogMessage);
                    });

                    await _hubConnection.StartAsync(token);
                    Log("成功连接到服务器！正在等待指令...");

                    var tcs = new TaskCompletionSource<object>();
                    token.Register(() => tcs.TrySetResult(null!));
                    await tcs.Task;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log($"连接失败: {ex.Message}");
                }
                if (!token.IsCancellationRequested)
                {
                    int retryDelay = 10000;
                    Log($"将在 {retryDelay / 1000} 秒后重试...");
                    try
                    {
                        await Task.Delay(retryDelay, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private async Task RemoveContentFromFile(string filePath, string contentToRemove, string logMessage)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Log($"文件不存在，无法删除内容: {filePath}");
                    return;
                }
                string originalContent = await File.ReadAllTextAsync(filePath);
                if (!originalContent.Contains(contentToRemove))
                {
                    Log("文件内容中不包含要删除的内容，跳过操作。");
                    return;
                }
                if(string.IsNullOrEmpty(originalContent)|| string.IsNullOrEmpty(contentToRemove))
                {
                    Log("文件内容或要删除的内容为空，跳过操作。");
                    return;
                }
                string newContent = originalContent.Replace(contentToRemove, string.Empty);
                await File.WriteAllTextAsync(filePath, newContent);
                Log(logMessage);
            }
            catch (Exception ex)
            {
                Log($"删除文件内容失败，文件: {filePath}, 错误: {ex.Message}");
            }
        }

        private async Task ModifyFile_Append(string filePath, string content, string logMessage)
        {
            try
            {
                string? directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Log($"创建目录: {directoryPath}");
                }
                string originalContent = await File.ReadAllTextAsync(filePath);
                if (originalContent.Contains(content))
                {
                    Log($"文件已存在相同内容，跳过写入: {filePath}");
                    return;
                }
                await File.AppendAllTextAsync(filePath, content);
                Log(logMessage);
            }
            catch (Exception ex)
            {
                Log($"修改文件失败，文件: {filePath}, 错误: {ex.Message}");
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var configToSave = new ConnectionConfig
                {
                    ServerUrl = ServerUrlTextBox.Text,
                    ApiKey = ApiKeyTextBox.Text
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(configToSave, options);

                string configFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configFileName);
                await File.WriteAllTextAsync(configFilePath, jsonString);
                Log("配置已成功保持到 config.json 文件.");
            }
            catch (Exception ex)
            {
                Log($"保存配置文件时发生错误: {ex.Message}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = string.Empty;
        }

        private void Log(string message)
        {
            string formattedMessage = $"{DateTime.Now} - {message}";
            _fileLogger?.Information(message);
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(formattedMessage + Environment.NewLine);
                LogTextBox.ScrollToEnd();
            });
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }

    }
}