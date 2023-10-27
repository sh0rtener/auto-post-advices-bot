using Autoposter.DiscordBot.Contracts;
using Discord;
using Discord.Commands;

namespace Autoposter.DiscordBot.Services
{
    public class ConsoleLogger : ILogger
    {
        public Task Log(LoggerLevel level, string message)
        {
            Console.WriteLine("{0}: {1}", level.ToString(), message);

            return Task.CompletedTask;
        }
    }
}
