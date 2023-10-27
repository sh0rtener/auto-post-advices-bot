using System.ComponentModel.DataAnnotations.Schema;

namespace Autoposter.DomainLayer.Entities.Autoposter
{
    public class BranchesRoles : EntityBase
    {
        [ForeignKey("branch_id")]
        public Branch? Branch { get; set; }
        public ulong RoleId { get; set; }
    }
}
