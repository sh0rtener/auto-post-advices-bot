using Autoposter.BotDiscord.Attributes;
using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Autoposter.BotDiscord.Modules.BotModules
{
    public class BotSettingsModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private IBotSettingsService _botSettingsService;
        private AppDbContext _context;
        private ILogger<BotSettingsModule> _logger;

        public BotSettingsModule(InteractionHandler handler, IBotSettingsService botSettingsService, AppDbContext context, ILogger<BotSettingsModule> logger)
        {
            _handler = handler;
            _botSettingsService = botSettingsService;
            _context = context;
            _logger = logger;
        }

        [RequireAdminRoles]
        [SlashCommand("изменить-задержку", "Изменить задержку между автопостингом")]
        public async Task ChangeAutopostDurationAsync([Summary(name: "минуты")] int interval)
        {
            BotSettings? botSettings = await _botSettingsService.GetBotSettingsAsync(Context.Guild.Id);
            await _botSettingsService.ChangeIntervalAsync(botSettings, interval);

            _logger.LogInformation($"The admin(id: {Context.User.Id}, guild_id: {Context.User.MutualGuilds.FirstOrDefault()!.Id})" +
                $" change the inverval  to {interval}");

            await RespondAsync($"Интервал успешно сменен на {interval}", ephemeral: true);
        }

        [RequireAdminRoles]
        [SlashCommand("роль", "Назначить роль, у которой будет доступ к боту")]
        public async Task SetAccessRoleAsync([Summary(name: "имя_роли")] string roleName)
        {
            SocketRole? role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName && Context.Guild.Id == x.Guild.Id);
            if (role is null)
            {
                await RespondAsync($"Указанной роли не существует", ephemeral: true);
                return;
            }

            await _botSettingsService.AddRoleAsync(new BotRole() { Id = Guid.NewGuid(), RoleId = role.Id, GuildId = Context.Guild.Id });

            _logger.LogInformation($"The admin(id: {Context.User.Id}, guild_id: {Context.User.MutualGuilds.FirstOrDefault()!.Id})" +
                $" set a role for bot permissions with name {roleName}");

            await RespondAsync($"Роль **{role.Name}** успешно добавлена!", ephemeral: true);
        }
    }
}
