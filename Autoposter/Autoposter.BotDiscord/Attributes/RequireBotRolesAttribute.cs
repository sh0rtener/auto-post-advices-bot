using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Autoposter.BotDiscord.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequireBotRolesAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            AppDbContext dbContext = services.GetRequiredService<AppDbContext>();
            var user = (SocketUser)await context.Client.GetUserAsync(context.User.Id);
            var guilds = user.MutualGuilds.FirstOrDefault();
            var userRoles = guilds!.Users.FirstOrDefault(x => x.Id == user.Id)!.Roles.ToList();

            List<BotRole> roles = await dbContext.BotRoles.ToListAsync();
            bool notInRole = roles.Select(x => x.RoleId).Intersect(userRoles.Select(x => x.Id)).Count() < 1;

            if (notInRole)
            {
                await context.Interaction.RespondAsync("У вас нет доступа", ephemeral: true);
                // return PreconditionResult.FromSuccess();
                return PreconditionResult.FromError($"The user {user.Username}(id: {user.Id}) doesn't have" +
                    $" important roles to use the commands");
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
