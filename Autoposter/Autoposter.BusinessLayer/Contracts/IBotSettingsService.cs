using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;

namespace Autoposter.BusinessLayer.Contracts
{
    public interface IBotSettingsService
    {
        Task<double> GetIntervalAsync(ulong guildId);
        Task<BotSettings> GetBotSettingsAsync(ulong guildId); 
        Task<DatabaseResult> ChangeIntervalAsync(BotSettings settings, int interval);
        Task<DatabaseResult> AddRoleAsync(BotRole botRole);
    }
}
