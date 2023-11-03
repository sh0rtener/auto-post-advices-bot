using Autoposter.BotDiscord.Attributes;
using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Autoposter.BotDiscord.Modules.ServerModules
{
    public class ServerModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private readonly AppDbContext _context;
        private ILogger<ServerModule> _logger;

        public ServerModule(InteractionHandler handler, AppDbContext context, ILogger<ServerModule> logger)
        {
            _handler = handler;
            _context = context;
            _logger = logger;
        }

        [RequireAdminRoles]
        [SlashCommand("добавить-сервер", "Позволяет администратору добавить новый сервер для постинга")]
        public async Task MakeNewBranchAsync(string serverName)
        {
            List<Server> channels = await _context.Servers.Where(x => x.Name == serverName).ToListAsync();

            Server? channel = channels.FirstOrDefault(x => x.Name == serverName && x.GuildId == Context.Guild.Id);

            if (channel is not null)
            {
                await RespondAsync($"Ошибка! Такой сервер уже существует", ephemeral: true);
                return;
            }

            await _context.Servers.AddAsync(new Server() { Id = Guid.NewGuid(), Name = serverName, GuildId = Context.Guild.Id });
            await _context.SaveChangesAsync();

            _logger.LogWarning($"The admin (id: {Context.User.Id}) add the server with name {serverName}");

            await RespondAsync("Сервер успешно добавлен!", ephemeral: true);
        }
    }
}
