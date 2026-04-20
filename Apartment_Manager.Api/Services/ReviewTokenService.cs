using Apartment_Manager.Api.Infrastructure;
using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Api.Services;

public class ReviewTokenService(IReviewTokenRepository repo) : IReviewTokenService
{
    public async Task<string> GenerateAsync(GenerateTokenRequest request)
    {
        var token = await repo.CreateAsync(request.ApartmentId, TimeSpan.FromDays(request.ExpiryDays));
        return token.ToString();
    }

    public async Task<TokenValidationResult> ValidateAsync(string rawToken)
    {
        if (!Guid.TryParse(rawToken, out var tokenGuid))
            return new TokenValidationResult(false, null);

        var token = await repo.GetByTokenAsync(tokenGuid);
        if (token is null || token.Used || token.ExpiresAt < DateTime.UtcNow)
            return new TokenValidationResult(false, null);

        return new TokenValidationResult(true, token.ApartmentId);
    }
}
