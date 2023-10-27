using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BusinessLayer.Validations
{
    public class DiscordRoleValidator
    {
        private readonly AppDbContext _context;
        public DiscordRoleValidator(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Validate(IEnumerable<SocketRole> roles)
        {
            List<BotRole> availableRoles = await _context.BotRoles.ToListAsync();
            return availableRoles.Select(x => x.RoleId).Intersect(roles.Select(x => x.Id)).Count() < 1;
        }
    }
}
