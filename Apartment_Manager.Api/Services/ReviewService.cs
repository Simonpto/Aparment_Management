using Apartment_Manager.Api.Infrastructure;
using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Api.Services;

public class ReviewService(IReviewRepository repo, IReviewTokenRepository tokenRepo) : IReviewService
{
    public Task<List<ReviewDto>> GetApprovedByApartmentAsync(Guid apartmentId) => repo.GetApprovedByApartmentAsync(apartmentId);
    public Task<List<AdminReviewDto>> GetAllAsync() => repo.GetAllAsync();

    public async Task<ReviewDto> SubmitAsync(CreateReviewRequest request)
    {
        if (!Guid.TryParse(request.Token, out var tokenGuid))
            throw new InvalidOperationException("Invalid token format.");

        var token = await tokenRepo.GetByTokenAsync(tokenGuid)
            ?? throw new InvalidOperationException("Token not found.");

        if (token.Used)                        throw new InvalidOperationException("This review link has already been used.");
        if (token.ExpiresAt < DateTime.UtcNow) throw new InvalidOperationException("This review link has expired.");

        var review = await repo.CreateAsync(token.ApartmentId, request);
        await tokenRepo.MarkUsedAsync(token.Id);
        return review;
    }

    public Task<bool> ApproveAsync(Guid id) => repo.ApproveAsync(id);
    public Task<bool> DeleteAsync(Guid id)  => repo.DeleteAsync(id);
}
