using Discord;
using Microsoft.Extensions.Logging;

namespace Autoposter.BotDiscord.Services
{
    public class NLogDiscordLogger
    {
        private Dictionary<LogSeverity, Action<string>> _loggerCommands = new Dictionary<LogSeverity, Action<string>>();
        private readonly ILogger<NLogDiscordLogger> _logger;
        public NLogDiscordLogger(ILogger<NLogDiscordLogger> logger)
        {
            _logger = logger;
            _loggerCommands.Add(LogSeverity.Critical, (message) => { _logger.LogCritical(message); });
            _loggerCommands.Add(LogSeverity.Error, (message) => { _logger.LogError(message); });
            _loggerCommands.Add(LogSeverity.Warning, (message) => { _logger.LogWarning(message); });
            _loggerCommands.Add(LogSeverity.Info, (message) => { _logger.LogInformation(message); });
            _loggerCommands.Add(LogSeverity.Verbose, (message) => { _logger.LogTrace(message); });
            _loggerCommands.Add(LogSeverity.Debug, (message) => { _logger.LogDebug(message); });
        }
        public Task Log(LogMessage message)
        {
            _loggerCommands[message.Severity](message.Message);
            return Task.CompletedTask;
        }
    }
}
