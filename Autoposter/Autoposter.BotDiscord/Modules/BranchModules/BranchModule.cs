using Autoposter.BotDiscord.Attributes;
using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Autoposter.BotDiscord.Modules.BranchModules
{
    public class BranchModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private IBranchService _branchService;
        private AppDbContext _context;
        private ILogger<BranchModule> _logger;

        public BranchModule(InteractionHandler handler, IBranchService branchService, AppDbContext context, ILogger<BranchModule> logger)
        {
            _handler = handler;
            _branchService = branchService;
            _context = context;
            _logger = logger;
        }

        [RequireAdminRoles]
        [SlashCommand("добавить-ветвь", "Позволяет администратору добавить новую ветвь для постинга")]
        public async Task MakeNewBranchAsync([Summary(name: "имя_ветки")] string branchName)
        {
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
            await _branchService.AddBranchAsync(branch);

            _logger.LogInformation($"The admin(id: {Context.User.Id}) add a branch with name {branchName}");

            await RespondAsync($"Ветвь успешно добавлена!", ephemeral: true);
        }


        [RequireAdminRoles]
        [SlashCommand("удалить-ветвь", "Позволяет администратору удалить новую ветвь для постинга")]
        public async Task RemoveBranchAsync([Summary(name: "имя_ветки")] string branchName)
        {
            List<SocketGuildChannel> channels = Context.Guild.Channels
                .Where(x => (x is SocketTextChannel && !(x is SocketVoiceChannel)) && x.Guild.Id == Context.Guild.Id)
                .ToList();

            SocketGuildChannel? channel = channels.FirstOrDefault(x => x.Name == branchName && x.Guild.Id == Context.Guild.Id);

            if (channel is null)
            {
                await RespondAsync($"Ошибка! такой ветви не существует", ephemeral: true);
                return;
            }

            Branch? branch = await _context.Branches.FirstOrDefaultAsync(x => x.Name == branchName && x.GuildId == Context.Guild.Id);
            if (branch is null)
            {
                await RespondAsync($"Ошибка! Ветка не была добавлена в бота", ephemeral: true);
                return;
            }
            _context.Branches.Remove(branch);

            _logger.LogInformation($"The admin(id: {Context.User.Id}) remove a branch with name {branchName}");

            await RespondAsync($"Ветвь успешно удалена!", ephemeral: true);
        }

        [RequireAdminRoles]
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
                .Where(x => (x is SocketTextChannel && !(x is SocketVoiceChannel)) && x.Guild.Id == Context.Guild.Id)
                .ToList();

            if (channels.FirstOrDefault(x => x.Name == branchName) is null)
            {
                await RespondAsync($"Ошибка! такой ветви не существует", ephemeral: true);
                return;
            }

            DatabaseResult result = await _branchService.AddBindingAsync(new BranchesRoles()
            {
                Id = Guid.NewGuid(),
                Branch = await _branchService.GetBranchAsync(branchName, Context.Guild.Id),
                RoleId = role.Id,
                GuildId = Context.Guild.Id
            });


            if (result == DatabaseResult.Null)
            {
                await RespondAsync($"Ошибка! Данная ветка не была занесена в торговые ветки", ephemeral: true);
                return;
            }

            _logger.LogInformation($"The admin(id: {Context.User.Id}) add a bind to branch with name {branchName} and role {roleName}");

            await RespondAsync($"Привязка успешно добавлена!", ephemeral: true);
        }

        [RequireAdminRoles]
        [SlashCommand("удалить-привязку", "Удалить привязку к текстовому каналу.")]
        public async Task RemoveChannelBindingAsync([Summary("роль")] string roleName, [Summary("ветка")] string branchName)
        {
            SocketRole? role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
            if (role is null)
            {
                await RespondAsync($"Указанной роли не существует", ephemeral: true);
                return;
            }

            if (await _branchService.RemoveBindingAsync(role.Id, branchName, Context.Guild.Id) is DatabaseResult.Null)
            {
                await RespondAsync($"Ошибка! Данной привязки не существует", ephemeral: true);
                return;
            }

            _logger.LogInformation($"The admin(id: {Context.User.Id}) remove a bind to branch with name {branchName} and role {roleName}");

            await RespondAsync($"Привязка успешно удалена!", ephemeral: true);
        }
    }
}
