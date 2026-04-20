using System.Net;
using System.Net.Http.Json;

namespace Apartment_Manager.Tests.Integration;

public class AdminEndpointTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    // ── POST /api/admin/login ────────────────────────────────────────

    [Fact]
    public async Task Login_ReturnsOk_WithCorrectPassword()
    {
        var response = await Client.PostAsJsonAsync("/api/admin/login",
            new { password = "test-admin-password" });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WithWrongPassword()
    {
        var response = await Client.PostAsJsonAsync("/api/admin/login",
            new { password = "wrong" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── GET /api/admin/me ────────────────────────────────────────────

    [Fact]
    public async Task Me_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        var anonClient = Factory.CreateClient();
        var response = await anonClient.GetAsync("/api/admin/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ReturnsOk_WhenLoggedIn()
    {
        await LoginAsAdminAsync();
        var response = await Client.GetAsync("/api/admin/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── POST /api/admin/logout ───────────────────────────────────────

    [Fact]
    public async Task Logout_ClearsSession()
    {
        await LoginAsAdminAsync();
        Assert.Equal(HttpStatusCode.OK, (await Client.GetAsync("/api/admin/me")).StatusCode);

        await Client.PostAsync("/api/admin/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, (await Client.GetAsync("/api/admin/me")).StatusCode);
    }

    // ── Protected routes require auth ────────────────────────────────

    [Theory]
    [InlineData("POST",   "/api/apartments")]
    [InlineData("DELETE", "/api/apartments/00000000-0000-0000-0000-000000000001")]
    [InlineData("POST",   "/api/blog")]
    [InlineData("GET",    "/api/blog/all")]
    [InlineData("GET",    "/api/reviews/admin")]
    [InlineData("POST",   "/api/review-tokens")]
    public async Task ProtectedEndpoint_Returns401_WhenUnauthenticated(string method, string path)
    {
        var anonClient = Factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { });

        var response = await anonClient.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
