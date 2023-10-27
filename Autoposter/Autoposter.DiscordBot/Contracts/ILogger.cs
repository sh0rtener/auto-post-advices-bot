namespace Autoposter.DiscordBot.Contracts
{
    public enum LoggerLevel
    {
        Info = 10 << 0,
        Warn = 10 << 1,
        Error = 10 << 2,
    }

    public interface ILogger
    {
        Task Log(LoggerLevel level, string message);
    }
}
