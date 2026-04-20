using System.Net.Http.Json;
using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Web.Services;

public class BlogApiService(HttpClient http) : IBlogApiService
{
    public async Task<List<BlogPostDto>> GetAllAsync() =>
        await http.GetFromJsonAsync<List<BlogPostDto>>("/api/blog") ?? [];

    public async Task<List<BlogPostDto>> GetAllAdminAsync() =>
        await http.GetFromJsonAsync<List<BlogPostDto>>("/api/blog/all") ?? [];

    public Task<BlogPostDetailDto?> GetBySlugAsync(string slug) =>
        http.GetFromJsonAsync<BlogPostDetailDto>($"/api/blog/slug/{slug}");

    public Task<BlogPostDetailDto?> GetByIdAsync(Guid id) =>
        http.GetFromJsonAsync<BlogPostDetailDto>($"/api/blog/{id}");

    public async Task<BlogPostDto> CreateAsync(CreateBlogPostRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/blog", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BlogPostDto>())!;
    }

    public async Task UpdateAsync(Guid id, UpdateBlogPostRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/blog/{id}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"/api/blog/{id}");
        response.EnsureSuccessStatusCode();
    }
}
