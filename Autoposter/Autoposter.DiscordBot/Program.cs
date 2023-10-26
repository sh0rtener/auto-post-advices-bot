using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Autoposter.DiscordBot;

class Program
{
    private readonly IServiceProvider _serviceProvider;
    public Program() =>
        _serviceProvider = CreateProvider();

    static void Main(string[] args) =>
        new Program().RunAsync(args).GetAwaiter().GetResult();

    private static IServiceProvider CreateProvider()
    {
        ServiceCollection builder = new ServiceCollection();

        builder.AddSingleton<DiscordSocketClient>();

        return builder.BuildServiceProvider();
    }

    private async Task RunAsync(string[] args)
    {
        DiscordSocketClient discordClient = _serviceProvider.GetRequiredService<DiscordSocketClient>();


    }
}
