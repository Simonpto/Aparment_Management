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

    public async Task UpsertAvailabilityAsync(Guid id, UpdateAvailabilityRequest request)
    {
        var response = await http.PutAsJsonAsync($"/api/apartments/{id}/availability", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<ApartmentImageDto> UploadImageAsync(Guid id, Stream imageStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(imageStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        var response = await http.PostAsync($"/api/apartments/{id}/images", content);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ApartmentImageDto>())!;
    }

    public async Task DeleteImageAsync(Guid apartmentId, Guid imageId)
    {
        var response = await http.DeleteAsync($"/api/apartments/{apartmentId}/images/{imageId}");
        response.EnsureSuccessStatusCode();
    }
}
