using MembershipSystemWPF.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using Serilog.Core;
using System.IO;
using System.Text.Json;
using System.Windows;
using Forms = System.Windows.Forms;

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
        private CancellationTokenSource _connectionCts = null!;
        private Forms.NotifyIcon _notifyIcon = null!;
        public MainWindow()
        {
            InitializeComponent();
            InitializeLogger();

            _notifyIcon = new Forms.NotifyIcon
            {
                Icon = new System.Drawing.Icon("Resources/favicon.ico"),
                Visible = true,
                Text = "限时会员网关"
            };
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }
        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }
        private void InitializeLogger()
        {
            string logDirectory = Path.Combine(GetExeCurrentPath(), "Logs");
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
            if (string.IsNullOrEmpty(DeviceName.Text))
            {
                DeviceName.Text = "PC-" + Environment.MachineName;
            }
        }

        private async Task LoadConfigAndConnectAsync()
        {
            string configFilePath = Path.Combine(GetExeCurrentPath(), _configFileName);
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
                DeviceName.Text = config?.DeviceName ?? string.Empty;
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
            DeviceName.IsEnabled = false;
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
            DeviceName.IsEnabled = true;
            ApiKeyTextBox.IsEnabled = true;
            SaveButton.IsEnabled = true;
            Log("连接已手动断开.");
        }

        private async Task ConnectInLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                HubConnection? hubConnection = null;
                var connectionTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
                using var cancellationTokenRegistration = token.Register(() => connectionTcs.TrySetResult(token));
                try
                {
                    string serverUrl = Dispatcher.Invoke(() => ServerUrlTextBox.Text);
                    string apiKey = Dispatcher.Invoke(() => ApiKeyTextBox.Text);
                    string deviceName = Dispatcher.Invoke(() => DeviceName.Text);
                    string serverUrlFull = $"{serverUrl}/filePushHub";
                    string urlWithKey = $"{serverUrlFull}?apiKey={Uri.EscapeDataString(apiKey)}&deviceName={deviceName}";

                    hubConnection = new HubConnectionBuilder()
                        .WithUrl(urlWithKey)
                        .WithAutomaticReconnect()
                        .Build();

                    hubConnection.Closed += (error) =>
                    {
                        if (!token.IsCancellationRequested)
                        {
                            Log($"与服务器的连接意外断开: {error?.Message}");
                        }
                        connectionTcs.TrySetResult(null!);
                        return Task.CompletedTask;
                    };

                    hubConnection.On<FileWriteCommand>("ReceiveWriteCommand", async (command) =>
                    {
                        await ModifyFile_Append(command.FilePath, command.Content, command.LogMessage);
                    });

                    hubConnection.On<FileDeleteCommand>("ReceiveDeleteCommand", async (command) =>
                    {
                        await RemoveContentFromFile(command.FilePath, command.ContentToRemove, command.LogMessage);
                    });

                    Log("正在尝试连接到服务器...");
                    await hubConnection.StartAsync(token);
                    Log("成功连接到服务器！正在等待指令...");
                    await connectionTcs.Task;

                    //var tcs = new TaskCompletionSource<object>();
                    //token.Register(() => tcs.TrySetResult(null!));
                    //await tcs.Task;
                }
                catch (OperationCanceledException)
                {
                    Log("连接过程已取消。");
                    break;
                }
                catch (Exception ex)
                {
                    Log($"连接失败: {ex.Message}");
                }
                finally
                {
                    if (hubConnection != null)
                    {
                        await hubConnection.DisposeAsync();
                    }
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
                        Log("连接重试已取消。");
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
                if (string.IsNullOrEmpty(originalContent) || string.IsNullOrEmpty(contentToRemove))
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
                if (!File.Exists(filePath))
                {
                    await File.WriteAllTextAsync(filePath, string.Empty);
                    Log($"创建新文件: {filePath}");
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
                    DeviceName = DeviceName.Text,
                    ApiKey = ApiKeyTextBox.Text
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(configToSave, options);

                string configFilePath = Path.Combine(GetExeCurrentPath(), _configFileName);
                await File.WriteAllTextAsync(configFilePath, jsonString);
                Log($"配置已成功保持到 {configFilePath} 文件.");
            }
            catch (Exception ex)
            {
                Log($"保存配置文件时发生错误: {ex.Message}");
            }
        }

        private static string GetExeCurrentPath()
        {
            string originalExePath = Environment.ProcessPath!;
            if (originalExePath != null)
            {
                return Path.GetDirectoryName(originalExePath) ?? AppContext.BaseDirectory;
            }
            return AppContext.BaseDirectory;
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

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 销毁托盘图标
            if (_notifyIcon != null)
            {
                _notifyIcon.Dispose();
                _notifyIcon = null!; // 可选，好习惯
            }
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}