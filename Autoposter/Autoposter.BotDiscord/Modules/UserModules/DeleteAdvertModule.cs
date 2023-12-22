using Autoposter.BotDiscord.Attributes;
using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace Autoposter.BotDiscord.Modules.UserModules
{
    public class DeleteAdvertModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private IPostService _postService;
        private AppDbContext _context;
        private ILogger<DeleteAdvertModule> _logger;
        public DeleteAdvertModule(InteractionHandler handler, IPostService postService, AppDbContext context, ILogger<DeleteAdvertModule> logger)
        {
            _handler = handler;
            _postService = postService;
            _context = context;
            _logger = logger;
        }

        [RequireBotRoles]
        [SlashCommand("удалить-объявление", "Позволяет пользователю удалить объявление")]
        public async Task RemoveAdvertAsync()
        {
            await _postService.RemoveAllByUserId(Context.User.Id, Context.User.MutualGuilds.FirstOrDefault()!.Id);
            _logger.LogInformation($"Post succesfully removed! (user_id: {Context.User.Id}, guild_id: {Context.User.MutualGuilds.FirstOrDefault()!.Id})");
            await RespondAsync($"Объявление успешно удалено! ", ephemeral: true);
        }

        [RequireAdminRoles]
        [SlashCommand("удалить-объявление-пользователя", "Позволяет администратору удалить объявление по id пользователя")]
        public async Task RemoveAdvertByAdminAsync([Summary(name: "id_пользователя")] string userId, [Summary(name: "причина")] string reason)
        {
            userId = ParseUserId(userId);
            await _postService.RemoveAllByUserId(ulong.Parse(userId), Context.Guild.Id);
            await RespondAsync($"Объявление успешно удалено! ", ephemeral: true);

            SocketGuildUser user = Context.Guild.GetUser(ulong.Parse(userId));
            _logger.LogInformation($"Post succesfully removed by admin! (user_id: {userId}, " +
                $"guild_id: {Context.User.MutualGuilds.FirstOrDefault()!.Id}), reason: {reason}");
            if (user is not null)
                await UserExtensions.SendMessageAsync(user, text: $"Ваше объявление было снято администратором. Причина: {reason}");
        }

        private string ParseUserId(string rawUserId)
        {
            string result = rawUserId.Replace("@", "").Replace("<", "").Replace(">", "");
            return result;
        }
    }
}
