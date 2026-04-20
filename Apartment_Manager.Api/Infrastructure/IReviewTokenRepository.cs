namespace Apartment_Manager.Api.Infrastructure;

public interface IReviewTokenRepository
{
    Task<Guid> CreateAsync(Guid apartmentId, TimeSpan expiresIn);
    Task<TokenData?> GetByTokenAsync(Guid token);
    Task MarkUsedAsync(Guid id);
}
