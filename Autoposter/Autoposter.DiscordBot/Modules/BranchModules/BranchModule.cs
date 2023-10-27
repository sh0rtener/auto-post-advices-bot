using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DiscordBot.Services;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Autoposter.DiscordBot.Modules.BranchModules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
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

            if (channel is null)
            {
                await RespondAsync($"Ошибка! такой ветви не существует", ephemeral: true);
                return;
            }

            await _context.Branches.AddAsync(new Branch() { Id = Guid.NewGuid(), BranchId = channel.Id, Name = branchName});
            await _context.SaveChangesAsync();

            await RespondAsync($"Ветвь успешно добавлена!", ephemeral: true);
        }
    }
}
