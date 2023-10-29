﻿using Autoposter.BusinessLayer.Data.EntityFramework;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BotDiscord.Attributes
{
    public class RequireAdminRoles : PreconditionAttribute
    {
        private readonly AppDbContext _dbContext = null!;
        public RequireAdminRoles() { }
        public RequireAdminRoles(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var user = (SocketUser)await context.Client.GetUserAsync(context.User.Id);
            var guilds = user.MutualGuilds.FirstOrDefault();
            var userRoles = guilds!.Users.FirstOrDefault(x => x.Id == user.Id)!.Roles.ToList();
            var adminRoles = context.Guild.Roles.Where(x => x.Permissions.Administrator).Select(x => x.Id);

            bool haveRole = adminRoles.Intersect(userRoles.Select(x => x.Id)).Count() < 1;

            if (!haveRole)
            {
                await context.User.SendMessageAsync("У вас нет доступа");
                return PreconditionResult.FromError($"The user {user.Username}(id: {user.Id}) doesn't have" +
                    $"permission to use admin commands");
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
