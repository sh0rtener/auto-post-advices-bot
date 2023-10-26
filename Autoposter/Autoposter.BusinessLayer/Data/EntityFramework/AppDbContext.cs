using Autoposter.DomainLayer.Entities.Autoposter;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BusinessLayer.Data.EntityFramework
{
    public class AppDbContext : DbContext
    {
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<PostsServers> PostsServers => Set<PostsServers>();
        public DbSet<PostsTags> PostsTags => Set<PostsTags>();
        public DbSet<Server> Servers => Set<Server>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<User> Users => Set<User>();

        public AppDbContext(DbContextOptions options) : base(options) { }
    }
}
