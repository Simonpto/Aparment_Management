using System.Net;
using System.Net.Http.Json;
using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Tests.Integration;

public class ApartmentsEndpointTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    // ── GET /api/apartments ──────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoApartments()
    {
        var result = await Client.GetFromJsonAsync<List<ApartmentDto>>("/api/apartments");

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAll_ReturnsApartments_WhenSomeExist()
    {
        await LoginAsAdminAsync();
        await Client.PostAsJsonAsync("/api/apartments", new CreateApartmentRequest("Sea View", null, "Myrina", null, null, null, null));
        await Client.PostAsJsonAsync("/api/apartments", new CreateApartmentRequest("Studio B", null, "Platy", null, null, null, null));

        var result = await Client.GetFromJsonAsync<List<ApartmentDto>>("/api/apartments");

        Assert.Equal(2, result!.Count);
    }

    // ── GET /api/apartments/{id} ─────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenNotExists()
    {
        var response = await Client.GetAsync($"/api/apartments/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsApartment_WhenExists()
    {
        await LoginAsAdminAsync();
        var created = await (await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("Mountain View", "Nice view", "Kontias", ["WiFi", "Pool"], 4, 2, 80m)))
            .Content.ReadFromJsonAsync<ApartmentDto>();

        var result = await Client.GetFromJsonAsync<ApartmentDto>($"/api/apartments/{created!.Id}");

        Assert.Equal("Mountain View", result!.Title);
        Assert.Equal("Kontias", result.Location);
        Assert.Equal(2, result.Amenities.Count);
        Assert.Equal(80m, result.BasePricePerNight);
    }

    // ── POST /api/apartments ─────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var response = await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("Test", null, null, null, null, null, null));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenAuthenticated()
    {
        await LoginAsAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("New Apt", "Desc", "Myrina", ["WiFi"], 2, 1, 60m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ApartmentDto>();
        Assert.NotNull(dto);
        Assert.Equal("New Apt", dto.Title);
        Assert.NotEqual(Guid.Empty, dto.Id);
    }

    // ── PUT /api/apartments/{id} ─────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsNoContent_WhenExists()
    {
        await LoginAsAdminAsync();
        var created = await (await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("Old Title", null, null, null, null, null, null)))
            .Content.ReadFromJsonAsync<ApartmentDto>();

        var response = await Client.PutAsJsonAsync($"/api/apartments/{created!.Id}",
            new UpdateApartmentRequest("New Title", null, null, null, null, null, null));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = await Client.GetFromJsonAsync<ApartmentDto>($"/api/apartments/{created.Id}");
        Assert.Equal("New Title", updated!.Title);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenNotExists()
    {
        await LoginAsAdminAsync();
        var response = await Client.PutAsJsonAsync($"/api/apartments/{Guid.NewGuid()}",
            new UpdateApartmentRequest("X", null, null, null, null, null, null));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── DELETE /api/apartments/{id} ──────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenExists()
    {
        await LoginAsAdminAsync();
        var created = await (await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("To Delete", null, null, null, null, null, null)))
            .Content.ReadFromJsonAsync<ApartmentDto>();

        var response = await Client.DeleteAsync($"/api/apartments/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await Client.GetAsync($"/api/apartments/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var response = await Client.DeleteAsync($"/api/apartments/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Availability ─────────────────────────────────────────────────

    [Fact]
    public async Task GetAvailability_ReturnsEmpty_WhenNoneSet()
    {
        await LoginAsAdminAsync();
        var apt = await (await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("Apt", null, null, null, null, null, null)))
            .Content.ReadFromJsonAsync<ApartmentDto>();

        var result = await Client.GetFromJsonAsync<List<AvailabilityDto>>($"/api/apartments/{apt!.Id}/availability");

        Assert.Empty(result!);
    }

    [Fact]
    public async Task UpsertAvailability_PersistsAndUpdates()
    {
        await LoginAsAdminAsync();
        var apt = await (await Client.PostAsJsonAsync("/api/apartments",
            new CreateApartmentRequest("Apt", null, null, null, null, null, null)))
            .Content.ReadFromJsonAsync<ApartmentDto>();
        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(10));

        await Client.PutAsJsonAsync($"/api/apartments/{apt!.Id}/availability",
            new UpdateAvailabilityRequest(date, false, 120m));

        var result = await Client.GetFromJsonAsync<List<AvailabilityDto>>($"/api/apartments/{apt.Id}/availability");
        var entry = Assert.Single(result!);
        Assert.Equal(date, entry.Date);
        Assert.False(entry.IsAvailable);
        Assert.Equal(120m, entry.PriceOverride);

        // Upsert — change to available
        await Client.PutAsJsonAsync($"/api/apartments/{apt.Id}/availability",
            new UpdateAvailabilityRequest(date, true, null));
        var updated = await Client.GetFromJsonAsync<List<AvailabilityDto>>($"/api/apartments/{apt.Id}/availability");
        Assert.True(Assert.Single(updated!).IsAvailable);
    }
}
