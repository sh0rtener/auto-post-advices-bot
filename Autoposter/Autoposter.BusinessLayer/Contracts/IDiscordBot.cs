using Discord.WebSocket;

namespace Autoposter.BusinessLayer.Contracts
{
    public interface IDiscordBot
    {
        Task Start();
        Task Initialize();
        Task HandleCommandAsync(SocketMessage arg);
        Task HandleSlashCommandAsync(SocketMessage arg);
    }
}
