using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Validations;
using Autoposter.DiscordBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.DiscordBot.Modules.UserModules
{
    public class DeleteAdvertUserModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private readonly AppDbContext _context;
        private InteractionHandler _handler;
        private DiscordRoleValidator _roleValidator;

        public DeleteAdvertUserModule(InteractionHandler handler, AppDbContext context, DiscordRoleValidator roleValidator)
        {
            _handler = handler;
            _context = context;
            _roleValidator = roleValidator;
        }

        [SlashCommand("удалить-объявление-пользователя", "Позволяет администратору удалить объявление по id пользователя")]
        public async Task RemoveAdvertByAdminAsync([Summary(name: "id_пользователя")] string userId, [Summary(name: "причина")] string reason)
        {
            var guilds = Context.User.MutualGuilds.FirstOrDefault()!;
            var userRoles = guilds
                .Users.FirstOrDefault(x => x.Id == Context.User.Id)!.Roles.ToList();

            if (await _roleValidator.Validate(userRoles))
            {
                await RespondAsync("Нет доступа!", ephemeral: true);
                return;
            }

            _context.Posts.RemoveRange(await _context.Posts.Where(
                x => x.DiscordId == ulong.Parse(userId) && x.GuildId == Context.Guild.Id).ToArrayAsync());
            await _context.SaveChangesAsync();

            await RespondAsync($"Объявление успешно удалено! ", ephemeral: true);

            SocketGuildUser user = Context.Guild.GetUser(ulong.Parse(userId));
            var userChannel = UserExtensions.SendMessageAsync(user, text: $"Ваше объявление было снято администратором. Причина: {reason}");
        }
    }
}
