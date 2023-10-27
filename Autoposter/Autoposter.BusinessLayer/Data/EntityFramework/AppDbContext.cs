using Autoposter.DomainLayer.Entities.Autoposter;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BusinessLayer.Data.EntityFramework
{
    public class AppDbContext : DbContext
    {
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Server> Servers => Set<Server>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<BotSettings> BotSettings => Set<BotSettings>();
        public DbSet<BotRole> BotRoles => Set<BotRole>();
        public DbSet<BranchRole> BranchRoles => Set<BranchRole>();

        public AppDbContext(DbContextOptions options) : base(options) { }
    }
}
