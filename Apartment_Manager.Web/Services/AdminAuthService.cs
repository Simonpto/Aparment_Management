using System.Net.Http.Json;

namespace Apartment_Manager.Web.Services;

public class AdminAuthService(HttpClient http) : IAdminAuthService
{
    public async Task<bool> LoginAsync(string password)
    {
        var response = await http.PostAsJsonAsync("/api/admin/login", new { password });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var response = await http.GetAsync("/api/admin/me");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await http.PostAsync("/api/admin/logout", null);
    }
}
