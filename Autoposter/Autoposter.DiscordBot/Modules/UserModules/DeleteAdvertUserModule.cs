using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DiscordBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Autoposter.DiscordBot.Modules.UserModules
{
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireOwner(Group = "Permission")]
    public class DeleteAdvertUserModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private readonly AppDbContext _context;
        private InteractionHandler _handler;

        public DeleteAdvertUserModule(InteractionHandler handler, AppDbContext context)
        {
            _handler = handler;
            _context = context;
        }

        [SlashCommand("удалить-объявление", "Позволяет пользователю удалить объявление")]
        public async Task RemoveAdvertAsync()
        {
            _context.Posts.RemoveRange(await _context.Posts.Where(x => x.DiscordId == Context.User.Id).ToArrayAsync());
            await _context.SaveChangesAsync();

            await RespondAsync($"Объявление успешно удалено! ", ephemeral: true);
        }

        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        [SlashCommand("удалить-объявление-пользователя", "Позволяет администратору удалить объявление по id пользователя")]
        public async Task RemoveAdvertByAdminAsync([Summary(name: "id_пользователя")] string userId, [Summary(name: "причина")] string reason)
        {
            _context.Posts.RemoveRange(await _context.Posts.Where(x => x.DiscordId == ulong.Parse(userId)).ToArrayAsync());
            await _context.SaveChangesAsync();

            await RespondAsync($"Объявление успешно удалено! ", ephemeral: true);

            SocketGuildUser user = Context.Guild.GetUser(ulong.Parse(userId));
            var userChannel = UserExtensions.SendMessageAsync(user, text: $"Ваше объявление было снято администратором. Причина: {reason}");
        }
    }
}
