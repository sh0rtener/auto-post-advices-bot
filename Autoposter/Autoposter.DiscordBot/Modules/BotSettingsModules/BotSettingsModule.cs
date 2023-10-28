using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Validations;
using Autoposter.DiscordBot.Services;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.DiscordBot.Modules.BotSettingsModules
{
    [RequireRole(roleName:"admin")]
    public class BotSettingsModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private readonly AppDbContext _context;
        private DiscordRoleValidator _roleValidator;

        public BotSettingsModule(InteractionHandler handler, AppDbContext context, DiscordRoleValidator roleValidator)
        {
            _handler = handler;
            _context = context;
            _roleValidator = roleValidator;
        }

        [SlashCommand("изменить-задержку", "Изменить задержку между автопостингом")]
        public async Task ChangeAutopostDurationAsync([Summary(name: "минуты")]int duration)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            BotSettings? botSettings = await _context.BotSettings
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id);

            if (botSettings is null)
            {
                botSettings = new BotSettings() { Id = Guid.NewGuid(), GuildId = Context.Guild.Id };
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
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            SocketRole? role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName && Context.Guild.Id == x.Guild.Id);
            if (role is null)
            {
                await RespondAsync($"Указанной роли не существует", ephemeral: true);
                return;
            }

            await _context.BotRoles.AddAsync(new BotRole() { Id = Guid.NewGuid(), RoleId = role.Id, GuildId = Context.Guild.Id });
            await _context.SaveChangesAsync();

            await RespondAsync($"Роль **{role.Name}** успешно добавлена!", ephemeral: true);
        }
    }
}
