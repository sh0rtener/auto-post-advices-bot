namespace Autoposter.BusinessLayer.Contracts
{
    public interface IDiscordParser
    {
        Task ParseBranchesAsync();
        Task ParseServersAsync();
    }
}
