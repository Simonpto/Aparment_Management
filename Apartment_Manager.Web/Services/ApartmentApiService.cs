using System.Net.Http.Json;
using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Web.Services;

public class ApartmentApiService(HttpClient http) : IApartmentApiService
{
    public async Task<List<ApartmentDto>> GetAllAsync() =>
        await http.GetFromJsonAsync<List<ApartmentDto>>("/api/apartments") ?? [];

    public Task<ApartmentDto?> GetByIdAsync(Guid id) =>
        http.GetFromJsonAsync<ApartmentDto>($"/api/apartments/{id}");

    public async Task<List<AvailabilityDto>> GetAvailabilityAsync(Guid id) =>
        await http.GetFromJsonAsync<List<AvailabilityDto>>($"/api/apartments/{id}/availability") ?? [];

    public async Task<List<ReviewDto>> GetReviewsAsync(Guid id) =>
        await http.GetFromJsonAsync<List<ReviewDto>>($"/api/apartments/{id}/reviews") ?? [];

    public async Task<ApartmentDto> CreateAsync(CreateApartmentRequest request)
    {
        var response = await http.PostAsJsonAsync("/api/apartments", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApartmentDto>())!;
    }

    public async Task UpdateAsync(Guid id, UpdateApartmentRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/apartments/{id}", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await http.DeleteAsync($"/api/apartments/{id}");
        response.EnsureSuccessStatusCode();
    }
}
