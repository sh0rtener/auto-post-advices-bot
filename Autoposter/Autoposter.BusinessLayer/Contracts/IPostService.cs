using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.Interactions;

namespace Autoposter.BusinessLayer.Contracts
{
    public interface IPostService
    {
        Task AddAsync(CreateAdvertModel model, SocketInteractionContext context, ulong guildId);
        Task<Post> GetLastByUserAsync(string discordId, ulong guildId);
        Task<int> TimeToCreate(ulong discordId, double time, ulong guildId);
        Task UpdateAsync(Post post, ulong guildId);
        Task RemoveAllByUserId(ulong discordId, ulong guildId);
    }
}
