using System.Net.Http.Json;
using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Web.Services;

public class ReviewApiService(HttpClient http) : IReviewApiService
{
    public async Task<TokenValidationResult> ValidateTokenAsync(string token) =>
        await http.GetFromJsonAsync<TokenValidationResult>($"/api/review-tokens/validate/{token}")
        ?? new TokenValidationResult(false, null);

    public async Task SubmitReviewAsync(CreateReviewRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/reviews", request);
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.Content.ReadAsStringAsync();
            throw new Exception(string.IsNullOrEmpty(msg) ? "Failed to submit review." : msg);
        }
    }

    public async Task<List<AdminReviewDto>> GetAllAdminAsync() =>
        await http.GetFromJsonAsync<List<AdminReviewDto>>("/api/reviews/admin") ?? [];

    public async Task ApproveAsync(Guid reviewId)
    {
        var response = await http.PostAsync($"/api/reviews/{reviewId}/approve", null);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteReviewAsync(Guid reviewId)
    {
        var response = await http.DeleteAsync($"/api/reviews/{reviewId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task<string> GenerateTokenAsync(GenerateTokenRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/review-tokens", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<string>())!;
    }
}
