using Autoposter.BotDiscord.Attributes;
using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.Interactions;
using Discord.WebSocket;

namespace Autoposter.BotDiscord.Modules.BranchModules
{
    [RequireAdminRoles]
    public class BranchModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private IBranchService _branchService;

        public BranchModule(InteractionHandler handler, IBranchService branchService)
        {
            _handler = handler;
            _branchService = branchService;
        }

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

            if (await _branchService.RemoveBindingAsync(role.Id, branchName, Context.Guild.Id) is DatabaseResult.Null)
            {
                await RespondAsync($"Ошибка! Данной привязки не существует", ephemeral: true);
                return;
            }

            await RespondAsync($"Привязка успешно удалена!", ephemeral: true);
        }
    }
}
