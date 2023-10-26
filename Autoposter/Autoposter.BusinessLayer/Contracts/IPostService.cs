using Autoposter.DomainLayer.Entities.Autoposter;

namespace Autoposter.BusinessLayer.Contracts
{
    public interface IPostService
    {
        Task AddAsync(Post post);
        Task<IEnumerable<Post>> GetByUserAsync(string discordId);
        Task UpdateAsync(Guid postId);
        Task RemoveAsync(Post post, User user);
    }
}
