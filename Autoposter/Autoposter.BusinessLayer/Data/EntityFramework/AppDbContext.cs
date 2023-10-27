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
        public DbSet<BranchesRoles> BranchesRoles => Set<BranchesRoles>();

        public AppDbContext(DbContextOptions options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BranchesRoles>().HasOne(x => x.Branch).WithMany(x => x.BranchRoles);
        }
    }
}
