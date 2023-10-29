using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BotDiscord.Attributes
{
    public class RequireBotRoles : PreconditionAttribute
    {
        private readonly AppDbContext _dbContext = null!;
        public RequireBotRoles() { }
        public RequireBotRoles(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var user = (SocketUser)await context.Client.GetUserAsync(context.User.Id);
            var guilds = user.MutualGuilds.FirstOrDefault();
            var userRoles = guilds!.Users.FirstOrDefault(x => x.Id == user.Id)!.Roles.ToList();

            List<BotRole> roles = await _dbContext.BotRoles.ToListAsync();
            bool haveRole = roles.Select(x => x.RoleId).Intersect(userRoles.Select(x => x.Id)).Count() < 1;

            if (!haveRole)
            {
                await context.User.SendMessageAsync("У вас нет доступа");
                return PreconditionResult.FromError($"The user {user.Username}(id: {user.Id}) doesn't have" +
                    $" important roles to use the commands");
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
