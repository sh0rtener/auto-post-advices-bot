using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.Interactions;

namespace Autoposter.BusinessLayer.Contracts
{
    public interface IPostService
    {
        Task AddAsync(CreateAdviceModel model, SocketInteractionContext context);
        Task<Post> GetLastByUserAsync(string discordId);
        Task<int> TimeToCreate(ulong discordId, double time);
        Task UpdateAsync(Post post);
        Task RemoveAllByUserId(ulong discordId);
    }
}
