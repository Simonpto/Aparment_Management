using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Web.Services;

public interface IApartmentApiService
{
    Task<List<ApartmentDto>> GetAllAsync();
    Task<ApartmentDto?> GetByIdAsync(Guid id);
    Task<List<AvailabilityDto>> GetAvailabilityAsync(Guid id);
    Task<List<ReviewDto>> GetReviewsAsync(Guid id);
    Task UpsertAvailabilityAsync(Guid id, UpdateAvailabilityRequest request);
    Task<ApartmentDto> CreateAsync(CreateApartmentRequest request);
    Task UpdateAsync(Guid id, UpdateApartmentRequest request);
    Task DeleteAsync(Guid id);
    Task<ApartmentImageDto> UploadImageAsync(Guid id, Stream imageStream, string fileName, string contentType);
    Task DeleteImageAsync(Guid apartmentId, Guid imageId);
}
