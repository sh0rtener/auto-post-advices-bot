using Autoposter.BusinessLayer.Contracts;
using Autoposter.BusinessLayer.Data.EntityFramework;
using Autoposter.BusinessLayer.Models;
using Autoposter.DomainLayer.Entities.Autoposter;
using Microsoft.EntityFrameworkCore;

namespace Autoposter.BusinessLayer.Realizations
{
    public class BranchService : IBranchService
    {
        private readonly AppDbContext _context;
        public BranchService(AppDbContext context) =>
            _context = context;

        public async Task<DatabaseResult> AddBindingAsync(BranchesRoles branchesRoles)
        {
            Branch? branch = await _context.Branches
                .FirstOrDefaultAsync(x => x.Name == branchesRoles!.Branch!.Name && x.GuildId == branchesRoles.GuildId);

            if (branch is null) return DatabaseResult.Null;

            await _context.BranchesRoles.AddAsync(branchesRoles);
            await _context.SaveChangesAsync();

            return DatabaseResult.Success;
        }

        public async Task<DatabaseResult> AddBranchAsync(Branch branch)
        {
            if (await _context.Branches.FirstOrDefaultAsync(x => x.Name == branch.Name) is null) return DatabaseResult.Conflict;

            await _context.Branches.AddAsync(branch);
            await _context.SaveChangesAsync();

            return DatabaseResult.Success;
        }

        public async Task<IEnumerable<Branch>> GetAvailableBranchesAsync(IEnumerable<ulong> roles, ulong guildId)
        {
            List<Branch> branches = await _context.Branches
                .AsNoTracking()
                .Where(x => x.GuildId == guildId)
                .Include(x => x.BranchRoles)
                .ToListAsync();

            List<Branch> branchesToRemove = new List<Branch>();

            foreach (Branch branch in branches)
            {
                bool isContain = false;
                foreach (ulong roleId in branch.BranchRoles.Select(x => x.RoleId))
                    if (roles.Contains(roleId)) { isContain = true; break;}

                if (!isContain && branch.BranchRoles.Count != 0) branchesToRemove.Add(branch);
            }

            return branches.Except(branchesToRemove);
        }

        public async Task<Branch?> GetBranchAsync(string branchName, ulong guildId)
        {
            return await _context.Branches.FirstOrDefaultAsync(x => x.Name == branchName && x.GuildId == guildId);
        }

        public async Task<IEnumerable<Branch>> GetBranchesAsync(ulong guildId)
        {
            IEnumerable<Branch> branches = await _context.Branches
                .AsNoTracking()
                .Where(x => x.GuildId == guildId)
                .Include(x => x.BranchRoles).ToListAsync();

            return branches;
        }

        public async Task<DatabaseResult> RemoveBindingAsync(ulong role, string branchName, ulong guildId)
        {
            BranchesRoles? branchRole = await _context.BranchesRoles
                .FirstOrDefaultAsync(x => x.Branch!.Name == branchName && x.RoleId == role && x.GuildId == guildId);
            if (branchRole is null)
                return DatabaseResult.Null;

            _context.BranchesRoles.Remove(branchRole);
            await _context.SaveChangesAsync();

            return DatabaseResult.Success;
        }
    }
}
