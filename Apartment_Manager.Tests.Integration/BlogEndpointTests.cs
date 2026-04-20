using System.Net;
using System.Net.Http.Json;
using Apartment_Manager.Shared.DTOs;

namespace Apartment_Manager.Tests.Integration;

public class BlogEndpointTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    // ── GET /api/blog ────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOnlyPublishedPosts()
    {
        await LoginAsAdminAsync();
        await Client.PostAsJsonAsync("/api/blog",
            new CreateBlogPostRequest("Draft Post", "draft-post", "<p>Draft</p>", null, null, null));
        var publishedResp = await Client.PostAsJsonAsync("/api/blog",
            new CreateBlogPostRequest("Published Post", "published-post", "<p>Published</p>", null, null, "Beaches"));
        var published = await publishedResp.Content.ReadFromJsonAsync<BlogPostDto>();
        await Client.PutAsJsonAsync($"/api/blog/{published!.Id}",
            new UpdateBlogPostRequest(null, null, null, null, null, Published: true));

        var result = await Client.GetFromJsonAsync<List<BlogPostDto>>("/api/blog");

        Assert.Single(result!);
        Assert.Equal("Published Post", result![0].Title);
    }

    [Fact]
    public async Task GetAll_ReturnsEmpty_WhenNoPosts()
    {
        var result = await Client.GetFromJsonAsync<List<BlogPostDto>>("/api/blog");
        Assert.Empty(result!);
    }

    // ── GET /api/blog/all (admin) ────────────────────────────────────

    [Fact]
    public async Task GetAllAdmin_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var response = await Client.GetAsync("/api/blog/all");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllAdmin_ReturnsAllPosts_WhenAuthenticated()
    {
        await LoginAsAdminAsync();
        await Client.PostAsJsonAsync("/api/blog",
            new CreateBlogPostRequest("Draft", "draft-1", null, null, null, null));
        await Client.PostAsJsonAsync("/api/blog",
            new CreateBlogPostRequest("Also Draft", "draft-2", null, null, null, null));

        var result = await Client.GetFromJsonAsync<List<BlogPostDto>>("/api/blog/all");
        Assert.Equal(2, result!.Count);
    }

    // ── GET /api/blog/slug/{slug} ────────────────────────────────────

    [Fact]
    public async Task GetBySlug_ReturnsNotFound_ForDraftPost()
    {
        await LoginAsAdminAsync();
        await Client.PostAsJsonAsync("/api/blog",
            new CreateBlogPostRequest("Hidden", "hidden-slug", "<p>secret</p>", null, null, null));

        var response = await Client.GetAsync("/api/blog/slug/hidden-slug");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBySlug_ReturnsPost_WhenPublished()
    {
        await LoginAsAdminAsync();
        var created = await (await Client.PostAsJsonAsync("/api/blog",
            new CreateBlogPostRequest("Limnos Beaches", "limnos-beaches", "<p>content</p>", "Short summary", null, "Beaches")))
            .Content.ReadFromJsonAsync<BlogPostDto>();
        await Client.PutAsJsonAsync($"/api/blog/{created!.Id}",
            new UpdateBlogPostRequest(null, null, null, null, null, Published: true));

        var result = await Client.GetFromJsonAsync<BlogPostDetailDto>("/api/blog/slug/limnos-beaches");

        Assert.Equal("Limnos Beaches", result!.Title);
        Assert.Equal("<p>content</p>", result.Content);
        Assert.Equal("Beaches", result.Category);
    }

    // ── POST + PUT + DELETE (admin) ──────────────────────────────────

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var response = await Client.PostAsJsonAsync("/api/blog",
            new CreateBlogPostRequest("Test", "test-slug", null, null, null, null));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesTitle_WhenAuthenticated()
    {
        await LoginAsAdminAsync();
        var created = await (await Client.PostAsJsonAsync("/api/blog",
            new CreateBlogPostRequest("Old", "update-slug", null, null, null, null)))
            .Content.ReadFromJsonAsync<BlogPostDto>();

        var response = await Client.PutAsJsonAsync($"/api/blog/{created!.Id}",
            new UpdateBlogPostRequest("New Title", null, null, null, null, null));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesPost_WhenAuthenticated()
    {
        await LoginAsAdminAsync();
        var created = await (await Client.PostAsJsonAsync("/api/blog",
            new CreateBlogPostRequest("To Delete", "delete-slug", null, null, null, null)))
            .Content.ReadFromJsonAsync<BlogPostDto>();

        var delResponse = await Client.DeleteAsync($"/api/blog/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delResponse.StatusCode);
        var all = await Client.GetFromJsonAsync<List<BlogPostDto>>("/api/blog/all");
        Assert.Empty(all!);
    }
}
