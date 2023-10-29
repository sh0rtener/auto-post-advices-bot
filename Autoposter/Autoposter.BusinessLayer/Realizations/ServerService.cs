using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DomainLayer.Entities.Autoposter;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BusinessLayer.Realizations
{
    public class ServerService : IServerService
    {
        private readonly AppDbContext _context;

        public ServerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Server>> GetServers(ulong guildId)
        {
            return await _context.Servers.Where(x => x.GuildId == guildId).ToListAsync();
        }
    }
}
