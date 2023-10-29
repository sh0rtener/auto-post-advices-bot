using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BotDiscord.Services
{
    public class AutoPoster
    {
        private AppDbContext _context;
        private DiscordSocketClient _client;
        private IConfiguration? _configuration;
        public AutoPoster(AppDbContext context, DiscordSocketClient client)
        {
            _context = context;
            _client = client;
        }

        public async Task StartPosting()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            await _context.Posts.ExecuteUpdateAsync(x => x.SetProperty(date => date.LastUpdateAt,
                date => DateTime.UtcNow));

            Console.WriteLine("start parsing");
            while (true)
            {
                await OnTimerElapsed();

                await Task.Delay(2000);
            }
        }

        private async Task OnTimerElapsed()
        {
            List<Post>? posts = _context!.Posts.AsNoTracking()
                .OrderByDescending(x => x.LastUpdateAt)
                .ToListAsync()
                .GetAwaiter().GetResult();



            foreach (Post post in posts)
            {
                double interval = await _context.BotSettings.Where(x => x.GuildId == post.GuildId)
                    .Select(x => x.Interval).OrderByDescending(x => x)
                    .FirstOrDefaultAsync();

                if (interval == 0) interval = double.MaxValue;

                if (post.BranchId is null && post.ServerId is null && post.ImageUri is null)
                    continue;

                if ((DateTime.UtcNow - post.LastUpdateAt).TotalMinutes <= interval) continue;
                else
                {
                    var embed = await GetEmbed(post);

                    await DoWork(embed, post);

                    await Task.Delay(200);
                }
            }
        }

        private async Task DoWork(Embed embed, Post post)
        {
            if (post.BranchId is null) return;
            var channel = _client!.GetChannel(ulong.Parse(post.BranchId!)) as IMessageChannel;

            if (channel is null) return;

            await channel!.SendMessageAsync(embed: embed);

            Post? oldPost = await _context!.Posts.FirstOrDefaultAsync(x => x.Id == post.Id);
            post.LastUpdateAt = DateTime.UtcNow;

            _context.Entry(oldPost!).CurrentValues.SetValues(post);
            await _context!.SaveChangesAsync();
        }

        private async Task<Embed> GetEmbed(Post post)
        {
            Guid serverId;
            Server? server = await _context!.Servers.FirstOrDefaultAsync();
            if (Guid.TryParse(post.ServerId!, out serverId))
                server = await _context!.Servers.FirstOrDefaultAsync(x => x.Id == serverId);
            string autoposterId = _configuration!["DiscordBot:WikiAutoPosterId"] ?? post.BranchId!;
            Uri branchUri = new Uri($"https://discord.com/channels/{_configuration!["DiscordBot:GuildsId"]}/" + autoposterId);

            _client!.Guilds.FirstOrDefault(x => x.Id == post.DiscordId);
            var user = await _client.GetUserAsync(post.DiscordId);

            var embed = new EmbedBuilder
            {
                //Title = $"{post.Name} предлагает свой аккаунт!",
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = user.GetAvatarUrl(),
                    Name = user.Username,
                },
                ThumbnailUrl = user.GetAvatarUrl(),
                Url = "https://discord.com/users/" + post.DiscordId,
                Color = 0x0099FF,
                Description = $"Ник: {post.Name}\nСервер: {server!.Name}\n\n" +
                            $"Объявление торговца:\n {post.Description}\n\nАктивировать услугу: \n{branchUri}" +
                            $"\n\nНаписать торговцу:\n {user.Mention}\n" +
                            $"Тег текстом:\n {user.Username}"
            };

            embed
                .WithImageUrl(post.ImageUri);

            return embed.Build();
        }
    }
}
