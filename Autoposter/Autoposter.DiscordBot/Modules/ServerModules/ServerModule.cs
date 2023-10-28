using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Validations;
using Autoposter.DiscordBot.Services;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.DiscordBot.Modules.ServerModules
{
    [RequireRole(roleName: "admin")]
    public class ServerModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private readonly AppDbContext _context;
        private DiscordRoleValidator _roleValidator;

        public ServerModule(InteractionHandler handler, AppDbContext context, DiscordRoleValidator roleValidator)
        {
            _handler = handler;
            _context = context;
            _roleValidator = roleValidator;
        }

        [SlashCommand("добавить-сервер", "Позволяет администратору добавить новый сервер для постинга")]
        public async Task MakeNewBranchAsync(string serverName)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

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
