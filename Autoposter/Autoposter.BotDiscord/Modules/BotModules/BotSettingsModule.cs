using Autoposter.BotDiscord.Attributes;
using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Contracts;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.Interactions;
using Discord.WebSocket;

namespace Autoposter.BotDiscord.Modules.BotModules
{
    [RequireAdminRoles()]
    public class BotSettingsModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private IBotSettingsService _botSettingsService;

        public BotSettingsModule(InteractionHandler handler, IBotSettingsService botSettingsService)
        {
            _handler = handler;
            _botSettingsService = botSettingsService;
        }

        [SlashCommand("изменить-задержку", "Изменить задержку между автопостингом")]
        public async Task ChangeAutopostDurationAsync([Summary(name: "минуты")] int interval)
        {
            BotSettings? botSettings = await _botSettingsService.GetBotSettingsAsync(Context.Guild.Id);
            await _botSettingsService.ChangeIntervalAsync(botSettings, interval);

            await RespondAsync($"Интервал успешно сменен с {botSettings.Interval} минут на {interval}", ephemeral: true);
        }

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

            await RespondAsync($"Роль **{role.Name}** успешно добавлена!", ephemeral: true);
        }
    }
}
