using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Api.Services;

public interface IReviewTokenService
{
    Task<string> GenerateAsync(GenerateTokenRequest request);
    Task<TokenValidationResult> ValidateAsync(string token);
}
