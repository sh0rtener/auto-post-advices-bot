using Autoposter.BotDiscord.Services;
using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Realizations;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args);

IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");

IConfiguration configuration = configurationBuilder.Build();

builder.ConfigureHostConfiguration(config => { config = configurationBuilder; });

builder.ConfigureServices((context, services) =>
{
    DiscordSocketConfig clientConfig = new DiscordSocketConfig
    {
        LogLevel = LogSeverity.Info,
        AlwaysDownloadUsers = true,

        MessageCacheSize = 100,
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildBans |
        GatewayIntents.MessageContent
    };

    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(context.Configuration["ConnectionStrings:Dev"],
        b => b.MigrationsAssembly("Autoposter.BotDiscord")).UseSnakeCaseNamingConvention());
    services.AddSingleton(clientConfig);
    services.AddSingleton<DiscordSocketClient>();
    services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
    services.AddSingleton<InteractionHandler>();
    services.AddTransient<DiscordLogger>();
    services.AddTransient<IPostService, PostService>();
    services.AddSingleton<AutoPoster>();

});

builder.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddNLog();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.Run();

using (var scope = app.Services.CreateScope())
{

    var client = scope.ServiceProvider.GetRequiredService<DiscordSocketClient>();
    var commands = scope.ServiceProvider.GetRequiredService<InteractionService>();
    var poster = scope.ServiceProvider.GetRequiredService<AutoPoster>();

    client.Log += _ => scope.ServiceProvider.GetRequiredService<DiscordLogger>().Log(_);
    commands.Log += _ => scope.ServiceProvider.GetRequiredService<DiscordLogger>().Log(_);
    client.Ready += ReadyAsync;

    await client.LoginAsync(TokenType.Bot, configuration["DiscordBot:Token"]);
    await client.StartAsync();

    await scope.ServiceProvider.GetRequiredService<InteractionHandler>()
        .InitializeAsync();

    await Task.Delay(2000);
    await poster.StartPosting();
    await Task.Delay(-1);

    async Task ReadyAsync()
    {
        if (IsDebug())
        {
            Console.WriteLine($"In debug mode, adding commands to guild id...");
            await commands.RegisterCommandsToGuildAsync(ulong.Parse(configuration["DiscordBot:GuildsId"]!));
        }
        else
            await commands.RegisterCommandsGloballyAsync(true);

        Console.WriteLine($"Connected as -> [{client.CurrentUser}] :)");
    }
}

bool IsDebug()
{
#if DEBUG
    return true;
#else
            return false;
#endif
}