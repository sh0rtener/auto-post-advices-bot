using Autoposter.BotDiscord.Attributes;
using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Contracts;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Autoposter.BotDiscord.Modules.UserModules
{
    public class DeleteAdvertModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService? Commands { get; set; }
        private InteractionHandler _handler;
        private IPostService _postService;
        public DeleteAdvertModule(InteractionHandler handler, IPostService postService)
        {
            _handler = handler;
            _postService = postService;
        }

        [RequireBotRoles]
        [SlashCommand("удалить-объявление", "Позволяет пользователю удалить объявление")]
        public async Task RemoveAdvertAsync()
        {
            await _postService.RemoveAllByUserId(Context.User.Id, Context.Guild.Id);
            await RespondAsync($"Объявление успешно удалено! ", ephemeral: true);
        }

        [RequireAdminRoles]
        [SlashCommand("удалить-объявление-пользователя", "Позволяет администратору удалить объявление по id пользователя")]
        public async Task RemoveAdvertByAdminAsync([Summary(name: "id_пользователя")] string userId, [Summary(name: "причина")] string reason)
        {
            await _postService.RemoveAllByUserId(Context.User.Id, Context.Guild.Id);
            await RespondAsync($"Объявление успешно удалено! ", ephemeral: true);

            SocketGuildUser user = Context.Guild.GetUser(ulong.Parse(userId));
            await UserExtensions.SendMessageAsync(user, text: $"Ваше объявление было снято администратором. Причина: {reason}");
        }
    }
}
