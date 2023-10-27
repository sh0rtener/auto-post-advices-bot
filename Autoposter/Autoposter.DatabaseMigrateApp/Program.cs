using Autoposter.BusinessLayer.Data.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

ServiceCollection builder = new ServiceCollection();

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

builder.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration["ConnectionStrings:Dev"],
            b => b.MigrationsAssembly("Autoposter.DatabaseMigrateApp"))
            .UseSnakeCaseNamingConvention());

ServiceProvider provider = builder.BuildServiceProvider();

var context = provider.GetRequiredService<AppDbContext>();
context.Database.Migrate();