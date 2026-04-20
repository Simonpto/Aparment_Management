using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Api.Services;

public interface IReviewService
{
    Task<List<ReviewDto>> GetApprovedByApartmentAsync(Guid apartmentId);
    Task<List<AdminReviewDto>> GetAllAsync();
    Task<ReviewDto> SubmitAsync(CreateReviewRequest request);
    Task<bool> ApproveAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
