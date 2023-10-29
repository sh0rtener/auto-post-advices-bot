using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;

namespace Autoposter.BusinessLayer.Contracts
{
    public interface IBranchService
    {
        Task<IEnumerable<Branch>> GetAvailableBranchesAsync(IEnumerable<ulong> roles, ulong guildId);
        Task<IEnumerable<Branch>> GetBranchesAsync(ulong guildId);
        Task<DatabaseResult> AddBranchAsync(Branch branch);
        Task<Branch?> GetBranchAsync(string branchName, ulong guildId);
        Task<DatabaseResult> AddBindingAsync(BranchesRoles branchesRoles);
        Task<DatabaseResult> RemoveBindingAsync(ulong role, string branchName, ulong guildId);
    }
}
