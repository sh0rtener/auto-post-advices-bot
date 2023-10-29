using Autoposter.DomainLayer.Entities.Autoposter;

namespace Autoposter.BusinessLayer.Contracts
{
    public interface IServerService
    {
        Task<IEnumerable<Server>> GetServers(ulong guildId);
    }
}
