using Autoposter.BusinessLayer.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Autoposter.DatabaseMigrateApp
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            builder.UseNpgsql(configuration["ConnectionStrings:Dev"], b => b.MigrationsAssembly("Autoposter.DatabaseMigrateApp"));
            builder.UseSnakeCaseNamingConvention();

            return new AppDbContext(builder.Options);
        }
    }
}
