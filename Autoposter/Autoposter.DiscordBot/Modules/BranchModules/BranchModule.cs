using Autoposter.BusinessLayer.Data.EntityFramework;
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

        public BranchModule(InteractionHandler handler, AppDbContext context)
        {
            _handler = handler;
            _context = context;
        }

        [SlashCommand("добавить-ветвь", "Позволяет администратору добавить новую ветвь для постинга")]
        public async Task MakeNewBranchAsync([Summary(name: "имя_ветки")] string branchName)
        {
            List<SocketGuildChannel> channels = Context.Guild.Channels
                .Where(x => (x is SocketTextChannel && !(x is SocketVoiceChannel)))
                .ToList();

            SocketGuildChannel? channel = channels.FirstOrDefault(x => x.Name == branchName);
            List<ulong> botRoles = await _context.BotRoles.Select(x => x.RoleId).ToListAsync();

            if (channel is null)
            {
                await RespondAsync($"Ошибка! такой ветви не существует", ephemeral: true);
                return;
            }

            Branch branch = new Branch() { Id = Guid.NewGuid(), BranchId = channel.Id, Name = branchName };
            await _context.Branches.AddAsync(branch);

            foreach (ulong roleId in botRoles)
                await _context.BranchesRoles.AddAsync(new BranchesRoles() { Id = Guid.NewGuid(), Branch = branch, RoleId = roleId });

            await _context.SaveChangesAsync();
            await RespondAsync($"Ветвь успешно добавлена!", ephemeral: true);
        }

        [SlashCommand("добавить-привязку", "Добавить привязку к текстовому каналу.")]
        public async Task AddChannelBindingAsync([Summary("роль")] string roleName, [Summary("ветка")] string branchName)
        {
            SocketRole? role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role is null)
            {
                await RespondAsync($"Указанной роли не существует", ephemeral: true);
                return;
            }

            List<SocketGuildChannel> channels = Context.Guild.Channels
                .Where(x => (x is SocketTextChannel && !(x is SocketVoiceChannel)))
                .ToList();

            if (channels.FirstOrDefault(x => x.Name == branchName) is null)
            {
                await RespondAsync($"Ошибка! такой ветви не существует", ephemeral: true);
                return;
            }

            Branch? branch = await _context.Branches.FirstOrDefaultAsync(x => x.Name == branchName);

            if (channels.FirstOrDefault(x => x.Name == branchName) is null)
            {
                await RespondAsync($"Ошибка! Данная ветка не была занесена в торговые ветки", ephemeral: true);
                return;
            }

            await _context.BranchesRoles.AddAsync(new BranchesRoles() { Id = Guid.NewGuid(), Branch = branch, RoleId = role.Id });
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
                .FirstOrDefaultAsync(x => x.Branch!.Name == branchName && x.RoleId == role.Id);
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
