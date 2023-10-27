using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BusinessLayer.Realizations
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;

        public PostService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CreateAdviceModel model, SocketInteractionContext context)
        {
            Post post = new Post()
            {
                Id = Guid.NewGuid(),
                DiscordId = context.User.Id,
                TagName = context.User.Mention,
                Name = model.Nickname,
                Description = model.Description,
                LastUpdateAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = false
            };

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task<int> TimeToCreate(ulong discordId, double time)
        {
            Post? postOld = await _context.Posts
                .OrderByDescending(x => x.LastUpdateAt)
                .FirstOrDefaultAsync(x => x.DiscordId == discordId);

            if (postOld is not null)
            {
                if ((DateTime.UtcNow - postOld.CreatedAt).Minutes <= time)
                {
                    return (int)time - (DateTime.UtcNow - postOld.CreatedAt).Minutes;
                }
            }

            return -1;
        }

        public async Task<Post> GetLastByUserAsync(string discordId)
        {
            Post? post = await _context.Posts
                .OrderByDescending(x => x.LastUpdateAt)
                .FirstOrDefaultAsync(x => x.DiscordId == ulong.Parse(discordId));

            return post!;
        }

        public async Task RemoveAllByUserId(ulong discordId)
        {
            _context.Posts.RemoveRange(await _context.Posts.Where(x => x.DiscordId == discordId).ToArrayAsync());
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Post post)
        {
            Post? entityExist = await _context.Posts.FirstOrDefaultAsync(x => x.Id == post.Id);
            if (entityExist is null)
                throw new NullReferenceException();

            _context.Entry(entityExist).CurrentValues.SetValues(post);
            await _context.SaveChangesAsync();
        }
    }
}
