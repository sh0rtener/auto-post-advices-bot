using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Realizations;
using Autoposter.BusinessLayer.Validations;
using Autoposter.DiscordBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Autoposter.DiscordBot;

class Program
{
    private readonly IConfiguration _configuration;
    private DiscordSocketClient? _client;
    private InteractionService _commands;
    public Program()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        _commands = new InteractionService(_client);
    }

    static void Main(string[] args) =>
        new Program().RunAsync(args).Wait();

    private ServiceProvider CreateProvider()
    {
        ServiceCollection builder = new ServiceCollection();

        DiscordSocketConfig clientConfig = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,
            AlwaysDownloadUsers = true,
            MessageCacheSize = 100,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.GuildBans |
            GatewayIntents.MessageContent
        };

        builder
            .AddSingleton(_configuration)
            .AddSingleton(clientConfig)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddTransient<DiscordLogger>()
            .AddTransient<IPostService, PostService>()
            .AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_configuration["ConnectionStrings:Dev"],
                b => b.MigrationsAssembly("Autoposter.DiscordBot"))
                .UseSnakeCaseNamingConvention())
            .AddTransient<DiscordRoleValidator>()
            .AddSingleton<AutoPoster>();


        return builder.BuildServiceProvider();
    }

    private async Task ReadyAsync()
    {
        if (IsDebug())
        {
            Console.WriteLine($"In debug mode, adding commands to guild id...");
            await _commands.RegisterCommandsToGuildAsync(ulong.Parse(_configuration["DiscordBot:GuildsId"]!));
        }
        else
        {
            await _commands.RegisterCommandsGloballyAsync(true);
        }

        Console.WriteLine($"Connected as -> [{_client!.CurrentUser}] :)");
    }


    private async Task RunAsync(string[] args)
    {
        using (var services = CreateProvider())
        {
            var client = services.GetRequiredService<DiscordSocketClient>();
            var commands = services.GetRequiredService<InteractionService>();
            var poster = services.GetRequiredService<AutoPoster>();
            AppDbContext context = services.GetRequiredService<AppDbContext>();
            await TakeMigration(context);
            _commands = commands;
            _client = client;

            client.Log += _ => services.GetRequiredService<DiscordLogger>().Log(_);
            commands.Log += _ => services.GetRequiredService<DiscordLogger>().Log(_);
            client.Ready += ReadyAsync;

            await _client.LoginAsync(TokenType.Bot, _configuration["DiscordBot:Token"]);
            await _client.StartAsync();

            await services.GetRequiredService<InteractionHandler>()
                .InitializeAsync();

            await Task.Delay(2000);
            await poster.StartPosting();
            await Task.Delay(-1);
        }
    }

    private async Task TakeMigration(AppDbContext context)
    {
            if (context.Database.GetPendingMigrations().Any())
            {
                await context.Database.MigrateAsync();
            }
    }

    static bool IsDebug()
    {
        #if DEBUG
            return true;
        #else
            return false;
        #endif
    }
}
