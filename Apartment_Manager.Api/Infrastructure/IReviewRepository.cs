using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Api.Infrastructure;

public interface IReviewRepository
{
    Task<List<ReviewDto>> GetApprovedByApartmentAsync(Guid apartmentId);
    Task<List<AdminReviewDto>> GetAllAsync();
    Task<ReviewDto> CreateAsync(Guid apartmentId, CreateReviewRequest request);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
