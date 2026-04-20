using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Web.Services;

public interface IBlogApiService
{
    Task<List<BlogPostDto>> GetAllAsync();
    Task<List<BlogPostDto>> GetAllAdminAsync();
    Task<BlogPostDetailDto?> GetBySlugAsync(string slug);
    Task<BlogPostDetailDto?> GetByIdAsync(Guid id);
    Task<BlogPostDto> CreateAsync(CreateBlogPostRequest request);
    Task UpdateAsync(Guid id, UpdateBlogPostRequest request);
    Task DeleteAsync(Guid id);
}
