using Autoposter.BusinessLayer.Data.EntityFramework;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BotDiscord.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequireAdminRolesAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var user = (SocketUser)await context.Client.GetUserAsync(context.User.Id);
            var guilds = user.MutualGuilds.FirstOrDefault();
            var userRoles = guilds!.Users.FirstOrDefault(x => x.Id == user.Id)!.Roles.ToList();
            var adminRoles = context.Guild.Roles.Where(x => x.Permissions.Administrator).Select(x => x.Id);

            bool haveRole = adminRoles.Intersect(userRoles.Select(x => x.Id)).Count() < 1;

            if (!haveRole)
            {
                await context.Interaction.RespondAsync("У вас нет доступа");
                return PreconditionResult.FromError($"The user {user.Username}(id: {user.Id}) doesn't have" +
                    $"permission to use admin commands");
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
