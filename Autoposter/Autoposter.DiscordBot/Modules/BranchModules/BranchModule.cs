using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Validations;
using Autoposter.DiscordBot.Services;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.DiscordBot.Modules.BranchModules
{
    [RequireRole(roleName: "admin")]
    public class BranchModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private readonly AppDbContext _context;
        private DiscordRoleValidator _roleValidator;

        public BranchModule(InteractionHandler handler, AppDbContext context, DiscordRoleValidator roleValidator)
        {
            _handler = handler;
            _roleValidator = roleValidator;
            _context = context;
        }

        [SlashCommand("добавить-ветвь", "Позволяет администратору добавить новую ветвь для постинга")]
        public async Task MakeNewBranchAsync([Summary(name: "имя_ветки")] string branchName)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            List<SocketGuildChannel> channels = Context.Guild.Channels
                .Where(x => (x is SocketTextChannel && !(x is SocketVoiceChannel)) && x.Guild.Id == Context.Guild.Id)
                .ToList();

            SocketGuildChannel? channel = channels.FirstOrDefault(x => x.Name == branchName && x.Guild.Id == Context.Guild.Id);

            if (channel is null)
            {
                await RespondAsync($"Ошибка! такой ветви не существует", ephemeral: true);
                return;
            }

            Branch branch = new Branch() { Id = Guid.NewGuid(), BranchId = channel.Id, Name = branchName, GuildId = Context.Guild.Id };
            await _context.Branches.AddAsync(branch);

            await _context.SaveChangesAsync();
            await RespondAsync($"Ветвь успешно добавлена!", ephemeral: true);
        }

        [SlashCommand("добавить-привязку", "Добавить привязку к текстовому каналу.")]
        public async Task AddChannelBindingAsync([Summary("роль")] string roleName, [Summary("ветка")] string branchName)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            SocketRole? role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role is null)
            {
                await RespondAsync($"Указанной роли не существует", ephemeral: true);
                return;
            }

            List<SocketGuildChannel> channels = Context.Guild.Channels
                .Where(x => (x is SocketTextChannel && !(x is SocketVoiceChannel)) && x.Guild.Id == Context.Guild.Id)
                .ToList();

            if (channels.FirstOrDefault(x => x.Name == branchName) is null)
            {
                await RespondAsync($"Ошибка! такой ветви не существует", ephemeral: true);
                return;
            }

            Branch? branch = await _context.Branches.FirstOrDefaultAsync(x => x.Name == branchName && x.GuildId == Context.Guild.Id);

            if (channels.FirstOrDefault(x => x.Name == branchName && x.Guild.Id == Context.Guild.Id) is null)
            {
                await RespondAsync($"Ошибка! Данная ветка не была занесена в торговые ветки", ephemeral: true);
                return;
            }

            await _context.BranchesRoles.AddAsync(new BranchesRoles() { Id = Guid.NewGuid(), Branch = branch,
                RoleId = role.Id, GuildId = Context.Guild.Id
            });
            await _context.SaveChangesAsync();

            await RespondAsync($"Привязка успешно добавлена!", ephemeral: true);
        }

        [SlashCommand("удалить-привязку", "Удалить привязку к текстовому каналу.")]
        public async Task RemoveChannelBindingAsync([Summary("роль")] string roleName, [Summary("ветка")] string branchName)
        {
            SocketRole? role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role is null)
            {
                await RespondAsync($"Указанной роли не существует", ephemeral: true);
                return;
            }

            BranchesRoles? branchRole = await _context.BranchesRoles
                .FirstOrDefaultAsync(x => x.Branch!.Name == branchName && x.RoleId == role.Id && x.GuildId == Context.Guild.Id);
            if (branchRole is null)
            {
                await RespondAsync($"Ошибка! Данной привязки не существует", ephemeral: true);
                return;
            }

            _context.BranchesRoles.Remove(branchRole);
            await _context.SaveChangesAsync();

            await RespondAsync($"Привязка успешно удалена!", ephemeral: true);
        }
    }
}
