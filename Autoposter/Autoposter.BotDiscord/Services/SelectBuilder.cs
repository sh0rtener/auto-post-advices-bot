using Autoposter.DomainLayer.Entities.Autoposter;
using Discord;

namespace Autoposter.BotDiscord.Services
{
    public class SelectBuilder
    {
        public static SelectMenuBuilder CreateBranchesMenu(IEnumerable<Branch> branches)
        {
            SelectMenuBuilder selectBranch = new SelectMenuBuilder()
            {
                CustomId = "select-branch",
                Placeholder = "Выберите канал"
            };

            foreach (Branch branch in branches) selectBranch.AddOption(branch.Name, branch.BranchId.ToString());

            return selectBranch;
        }

        public static SelectMenuBuilder CreateServersMenu(IEnumerable<Server> servers)
        {
            SelectMenuBuilder selectBranch = new SelectMenuBuilder()
            {
                CustomId = "select-server",
                Placeholder = "Выберите сервер"
            };

            foreach (Server server in servers) selectBranch.AddOption(server.Name, server.Id.ToString());

            return selectBranch;
        }
    }
}
