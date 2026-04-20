using System.Net;
using System.Net.Http.Json;
using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Tests.Integration;

public class ReviewTokenEndpointTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    // ── GET /api/review-tokens/validate/{token} ──────────────────────

    [Fact]
    public async Task Validate_ReturnsFalse_ForRandomGuid()
    {
        var result = await Client.GetFromJsonAsync<TokenValidationResult>(
            $"/api/review-tokens/validate/{Guid.NewGuid()}");

        Assert.False(result!.IsValid);
        Assert.Null(result.ApartmentId);
    }

    [Fact]
    public async Task Validate_ReturnsFalse_ForNonGuid()
    {
        var result = await Client.GetFromJsonAsync<TokenValidationResult>(
            "/api/review-tokens/validate/not-a-guid");

        Assert.False(result!.IsValid);
    }

    [Fact]
    public async Task Validate_ReturnsTrue_ForValidUnusedToken()
    {
        await LoginAsAdminAsync();
        var apt = await (await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("Apt", null, null, null, null, null, null)))
            .Content.ReadFromJsonAsync<ApartmentDto>();

        var token = await (await Client.PostAsJsonAsync("/api/review-tokens",
            new GenerateTokenRequest(apt!.Id, 7)))
            .Content.ReadFromJsonAsync<string>();

        var result = await Client.GetFromJsonAsync<TokenValidationResult>(
            $"/api/review-tokens/validate/{token}");

        Assert.True(result!.IsValid);
        Assert.Equal(apt.Id, result.ApartmentId);
    }

    [Fact]
    public async Task Validate_ReturnsFalse_AfterTokenUsed()
    {
        await LoginAsAdminAsync();
        var apt = await (await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("Apt", null, null, null, null, null, null)))
            .Content.ReadFromJsonAsync<ApartmentDto>();

        var token = await (await Client.PostAsJsonAsync("/api/review-tokens",
            new GenerateTokenRequest(apt!.Id, 7)))
            .Content.ReadFromJsonAsync<string>();

        await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(token!, 5, "Great!", "Alice"));

        var result = await Client.GetFromJsonAsync<TokenValidationResult>(
            $"/api/review-tokens/validate/{token}");

        Assert.False(result!.IsValid);
    }

    // ── POST /api/review-tokens ──────────────────────────────────────

    [Fact]
    public async Task Generate_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var apt = Guid.NewGuid();
        var response = await Client.PostAsJsonAsync("/api/review-tokens",
            new GenerateTokenRequest(apt, 7));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Generate_ReturnsValidGuidToken_WhenAuthenticated()
    {
        await LoginAsAdminAsync();
        var apt = await (await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("Apt", null, null, null, null, null, null)))
            .Content.ReadFromJsonAsync<ApartmentDto>();

        var token = await (await Client.PostAsJsonAsync("/api/review-tokens",
            new GenerateTokenRequest(apt!.Id, 14)))
            .Content.ReadFromJsonAsync<string>();

        Assert.True(Guid.TryParse(token, out _));
    }
}
