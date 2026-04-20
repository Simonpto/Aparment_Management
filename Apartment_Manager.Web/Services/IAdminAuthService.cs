namespace Apartment_Manager.Web.Services;

public interface IAdminAuthService
{
    Task<bool> LoginAsync(string password);
    Task<bool> IsAuthenticatedAsync();
    Task LogoutAsync();
}
