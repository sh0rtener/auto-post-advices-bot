namespace Autoposter.DomainLayer.Entities.Autoposter
{
    public class Branch : EntityBase
    {
        public string? Name { get; set; }
        public ulong BranchId { get; set; }
        public ICollection<BranchesRoles> BranchRoles { get; } = new List<BranchesRoles>();
    }
}
