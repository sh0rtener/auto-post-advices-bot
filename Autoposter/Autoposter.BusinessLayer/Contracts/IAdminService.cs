namespace Autoposter.BusinessLayer.Contracts
{
    public interface IAdminService : IBindingService
    {
        Task GiveAccess(string roleName);
    }
}
