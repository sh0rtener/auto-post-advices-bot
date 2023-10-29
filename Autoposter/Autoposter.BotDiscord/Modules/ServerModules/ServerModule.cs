using Autoposter.BotDiscord.Attributes;
using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BotDiscord.Modules.ServerModules
{
    [RequireAdminRoles]
    public class ServerModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private readonly AppDbContext _context;

        public ServerModule(InteractionHandler handler, AppDbContext context)
        {
            _handler = handler;
            _context = context;
        }

        [SlashCommand("добавить-сервер", "Позволяет администратору добавить новый сервер для постинга")]
        public async Task MakeNewBranchAsync(string serverName)
        {
            List<Server> channels = await _context.Servers.Where(x => x.Name == serverName).ToListAsync();

            Server? channel = channels.FirstOrDefault(x => x.Name == serverName);

            if (channel is not null)
            {
                await RespondAsync($"Ошибка! Такой сервер уже существует", ephemeral: true);
                return;
            }

            await _context.Servers.AddAsync(new Server() { Id = Guid.NewGuid(), Name = serverName, GuildId = Context.Guild.Id });
            await _context.SaveChangesAsync();

            await RespondAsync("Сервер успешно добавлен!", ephemeral: true);
        }
    }
}
