using Microsoft.Extensions.Logging;

namespace PatchAgent.CustomerA.Utilities
{
    public class LoggingUtility
    {
        private readonly ILogger<LoggingUtility> _logger;

        public LoggingUtility(ILogger<LoggingUtility> logger)
        {
            _logger = logger;
        }

        public async Task LogToUserFileAsync(string message)
        {
            try
            {
                var logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PatchAgent");
                Directory.CreateDirectory(logDirectory);

                var logFile = Path.Combine(logDirectory, $"PatchLog_{DateTime.Now:yyyy-MM-dd}.txt");
                await File.AppendAllTextAsync(logFile, message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to write to user log file");
            }
        }

        public async Task LogInfoAsync(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
            await LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ℹ️ {string.Format(message, args)}");
        }

        public async Task LogWarningAsync(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
            await LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ⚠️ {string.Format(message, args)}");
        }

        public async Task LogErrorAsync(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, message, args);
            await LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ❌ {string.Format(message, args)} - {ex.Message}");
        }

        public async Task LogSuccessAsync(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
            await LogToUserFileAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ✅ {string.Format(message, args)}");
        }

        public async Task LogSeparatorAsync()
        {
            await LogToUserFileAsync("==================================================");
        }
    }
}