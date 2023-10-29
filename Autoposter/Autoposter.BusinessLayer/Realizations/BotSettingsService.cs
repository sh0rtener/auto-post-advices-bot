using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BusinessLayer.Realizations
{
    public class BotSettingsService : IBotSettingsService
    {
        private readonly AppDbContext _context;
        public BotSettingsService(AppDbContext context) =>
            _context = context;

        public async Task<DatabaseResult> AddRoleAsync(BotRole botRole)
        {
            await _context.BotRoles.AddAsync(botRole);
            await _context.SaveChangesAsync();

            return DatabaseResult.Success;
        }

        public async Task<DatabaseResult> ChangeIntervalAsync(BotSettings settings, int interval)
        {
            BotSettings? botSettings = await _context.BotSettings
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync(x => x.GuildId == settings.GuildId);

            if (botSettings is null)
            {
                botSettings = new BotSettings() { Id = Guid.NewGuid(), GuildId = settings.GuildId };
                await _context.BotSettings.AddAsync(botSettings);
            }

            int oldDuration = botSettings!.Interval;
            botSettings!.Interval = interval;
            await _context.SaveChangesAsync();

            return DatabaseResult.Success;
        }

        public async Task<BotSettings> GetBotSettingsAsync(ulong guildId)
        {
            BotSettings? botSettings = await _context.BotSettings
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync(x => x.GuildId == guildId);

            if (botSettings is null)
            {
                botSettings = new BotSettings() { Id = Guid.NewGuid(), GuildId = guildId, Interval = 0 };
                await _context.BotSettings.AddAsync(botSettings);
            }

            await _context.SaveChangesAsync();

            return botSettings;
        }

        public async Task<double> GetIntervalAsync(ulong guildId)
        {
            double interval = await _context.BotSettings.Where(x => x.GuildId == guildId)
                .Select(x => x.Interval).OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            return interval;
        }
    }
}
