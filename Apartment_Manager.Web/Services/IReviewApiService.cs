using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Web.Services;

public interface IReviewApiService
{
    Task<TokenValidationResult> ValidateTokenAsync(string token);
    Task SubmitReviewAsync(CreateReviewRequest request);
    Task<List<AdminReviewDto>> GetAllAdminAsync();
    Task ApproveAsync(Guid reviewId);
    Task DeleteReviewAsync(Guid reviewId);
    Task<string> GenerateTokenAsync(GenerateTokenRequest request);
}
