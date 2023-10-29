using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.DomainLayer.Entities.Autoposter;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Autoposter.BotDiscord.Models;
using Microsoft.Extensions.Logging;

namespace Autoposter.BotDiscord.Services
{
    public class AutoPoster
    {
        private AppDbContext _context;
        private DiscordSocketClient _client;
        private IConfiguration? _configuration;
        private ILogger<AutoPoster> _logger;
        public AutoPoster(AppDbContext context, DiscordSocketClient client, ILogger<AutoPoster> logger)
        {
            _context = context;
            _client = client;
            _logger = logger;
        }

        public async Task StartPosting()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            await _context.Posts.ExecuteUpdateAsync(x => x.SetProperty(date => date.LastUpdateAt,
                date => DateTime.UtcNow));

            _logger.LogInformation("Autoposter starting to post");
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
                    var embed = EmbedFactory.GetGeneralEmbed(await GetEmbedModelAsync(post));

                    await DoWork(embed, post);

                    _logger.LogInformation($"Autopost succesfully posted a post; (post_name: {post.Name}, user_id: {post.DiscordId})");

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

        private async Task<EmbedModel> GetEmbedModelAsync(Post post)
        {
            Guid serverId;
            Server? server = await _context!.Servers.FirstOrDefaultAsync();
            if (Guid.TryParse(post.ServerId!, out serverId))
                server = await _context!.Servers.FirstOrDefaultAsync(x => x.Id == serverId);
            var user = (SocketUser)await _client.GetUserAsync(post.DiscordId);

            EmbedModel embedModel = new EmbedModel
            {
                Post = post,
                User = user,
                Server = server!,
                GuildId = user.MutualGuilds.FirstOrDefault()!.Id
            };

            return embedModel;
        }
    }
}
