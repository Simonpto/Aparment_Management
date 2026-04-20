using System.Net;
using System.Net.Http.Json;
using Apartment_Manager.Shared.DTOs;
using Dapper;
using Npgsql;

namespace Apartment_Manager.Tests.Integration;

public class ReviewsEndpointTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    // ── Helpers ──────────────────────────────────────────────────────

    private async Task<ApartmentDto> CreateApartmentAsync(string title = "Test Apt")
    {
        await LoginAsAdminAsync();
        return (await (await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest(title, null, null, null, null, null, null)))
            .Content.ReadFromJsonAsync<ApartmentDto>())!;
    }

    private async Task<string> CreateTokenAsync(Guid apartmentId, int expiryDays = 7)
    {
        var response = await Client.PostAsJsonAsync("/api/review-tokens",
            new GenerateTokenRequest(apartmentId, expiryDays));
        return (await response.Content.ReadFromJsonAsync<string>())!;
    }

    private async Task InsertExpiredTokenAsync(Guid apartmentId)
    {
        using var conn = new NpgsqlConnection(Factory.GetConnectionString());
        await conn.OpenAsync();
        await conn.ExecuteAsync(
            "INSERT INTO review_tokens (apartment_id, expires_at) VALUES (@id, @exp)",
            new { id = apartmentId, exp = DateTime.UtcNow.AddDays(-1) });
    }

    // ── GET /api/apartments/{id}/reviews ─────────────────────────────

    [Fact]
    public async Task GetReviews_ReturnsOnlyApprovedReviews()
    {
        var apt = await CreateApartmentAsync();
        var token = await CreateTokenAsync(apt.Id);
        await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(token, 4, "Great stay!", "Alice"));

        var reviews = await Client.GetFromJsonAsync<List<ReviewDto>>($"/api/apartments/{apt.Id}/reviews");

        Assert.Empty(reviews!); // not approved yet
    }

    [Fact]
    public async Task GetReviews_ReturnsApprovedAfterApproval()
    {
        var apt = await CreateApartmentAsync();
        var token = await CreateTokenAsync(apt.Id);
        await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(token, 5, "Loved it!", "Bob"));

        var adminReviews = await Client.GetFromJsonAsync<List<AdminReviewDto>>("/api/reviews/admin");
        await Client.PostAsync($"/api/reviews/{adminReviews![0].Id}/approve", null);

        var publicReviews = await Client.GetFromJsonAsync<List<ReviewDto>>($"/api/apartments/{apt.Id}/reviews");
        Assert.Single(publicReviews!);
        Assert.Equal("Bob", publicReviews![0].GuestName);
    }

    // ── POST /api/reviews ────────────────────────────────────────────

    [Fact]
    public async Task Submit_ReturnsBadRequest_WithInvalidGuidToken()
    {
        var response = await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest("not-a-guid", 4, "Nice", "Eve"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_ReturnsBadRequest_WithUnknownToken()
    {
        var response = await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(Guid.NewGuid().ToString(), 4, "Nice", "Eve"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Submit_CreatesReview_WithValidToken()
    {
        var apt = await CreateApartmentAsync();
        var token = await CreateTokenAsync(apt.Id);

        var response = await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(token, 5, "Amazing!", "Carol"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Submit_ReturnsBadRequest_WhenTokenAlreadyUsed()
    {
        var apt = await CreateApartmentAsync();
        var token = await CreateTokenAsync(apt.Id);

        await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(token, 5, "First use", "Dan"));

        var secondResponse = await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(token, 4, "Second use", "Dan"));

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        Assert.Contains("already been used", await secondResponse.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Submit_ReturnsBadRequest_WithExpiredToken()
    {
        var apt = await CreateApartmentAsync();

        // Insert an expired token directly in the DB (bypassing the service)
        using var conn = new NpgsqlConnection(Factory.GetConnectionString());
        await conn.OpenAsync();
        var expiredToken = await conn.ExecuteScalarAsync<Guid>(
            "INSERT INTO review_tokens (apartment_id, expires_at) VALUES (@id, @exp) RETURNING token",
            new { id = apt.Id, exp = DateTime.UtcNow.AddDays(-1) });

        var response = await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(expiredToken.ToString(), 4, "Late", "Frank"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("expired", await response.Content.ReadAsStringAsync());
    }

    // ── Admin review management ──────────────────────────────────────

    [Fact]
    public async Task GetAdminReviews_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Create a fresh client without auth cookie
        var anonClient = Factory.CreateClient();
        var response = await anonClient.GetAsync("/api/reviews/admin");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Approve_SetsApprovedFlag()
    {
        var apt = await CreateApartmentAsync();
        var token = await CreateTokenAsync(apt.Id);
        await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(token, 3, "OK", "Grace"));

        var all = await Client.GetFromJsonAsync<List<AdminReviewDto>>("/api/reviews/admin");
        var id = all![0].Id;
        Assert.False(all[0].Approved);

        await Client.PostAsync($"/api/reviews/{id}/approve", null);

        var updated = await Client.GetFromJsonAsync<List<AdminReviewDto>>("/api/reviews/admin");
        Assert.True(updated![0].Approved);
    }

    [Fact]
    public async Task DeleteReview_RemovesIt()
    {
        var apt = await CreateApartmentAsync();
        var token = await CreateTokenAsync(apt.Id);
        await Client.PostAsJsonAsync("/api/reviews",
            new CreateReviewRequest(token, 2, "Meh", "Heidi"));

        var all = await Client.GetFromJsonAsync<List<AdminReviewDto>>("/api/reviews/admin");
        await Client.DeleteAsync($"/api/reviews/{all![0].Id}");

        var remaining = await Client.GetFromJsonAsync<List<AdminReviewDto>>("/api/reviews/admin");
        Assert.Empty(remaining!);
    }
}
