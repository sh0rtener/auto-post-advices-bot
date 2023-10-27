using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DiscordBot.Services;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.DiscordBot.Modules.BotSettingsModules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class BotSettingsModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private readonly AppDbContext _context;

        public BotSettingsModule(InteractionHandler handler, AppDbContext context)
        {
            _handler = handler;
            _context = context; 
        }

        [SlashCommand("изменить-задержку", "Изменить задержку между автопостингом")]
        public async Task ChangeAutopostDurationAsync([Summary(name: "минуты")]int duration)
        {
            BotSettings? botSettings = await _context.BotSettings
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            if (botSettings is null)
            {
                botSettings = new BotSettings() { Id = Guid.NewGuid() };
                await _context.BotSettings.AddAsync(botSettings);
            }

            int oldDuration = botSettings!.Interval;
            botSettings!.Interval = duration;
            await _context.SaveChangesAsync();

            await RespondAsync($"Интервал успешно сменен с {oldDuration} минут на {duration}", ephemeral: true);
        }

        [SlashCommand("роль", "Назначить роль, у которой будет доступ к боту")]
        public async Task SetAccessRoleAsync([Summary(name: "имя_роли")]string roleName)
        {
            SocketRole? role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role is null)
            {
                await RespondAsync($"Указанной роли не существует", ephemeral: true);
                return;
            }

            await _context.BotRoles.AddAsync(new BotRole() { Id = Guid.NewGuid(), RoleId = role.Id});
            await _context.SaveChangesAsync();

            await RespondAsync($"Роль **{role.Name}** успешно добавлена!", ephemeral: true);
        }
    }
}
