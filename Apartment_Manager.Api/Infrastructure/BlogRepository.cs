using Apartment_Manager.Shared.DTOs;
using Dapper;

namespace Apartment_Manager.Api.Infrastructure;

public class BlogRepository(DatabaseConnectionFactory db)
{
    private const string SelectSummary = "SELECT id, title, slug, excerpt, image_url AS ImageUrl, category, published_at AS PublishedAt FROM blog_posts";
    private const string SelectDetail  = "SELECT id, title, slug, content, excerpt, image_url AS ImageUrl, category, published, published_at AS PublishedAt FROM blog_posts";

    public async Task<List<BlogPostDto>> GetAllPublishedAsync()
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var rows = await conn.QueryAsync<BlogPostRow>($"{SelectSummary} WHERE published = true ORDER BY published_at DESC");
        return rows.Select(MapSummary).ToList();
    }

    public async Task<List<BlogPostDto>> GetAllAsync()
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var rows = await conn.QueryAsync<BlogPostRow>($"{SelectSummary} ORDER BY created_at DESC");
        return rows.Select(MapSummary).ToList();
    }

    public async Task<BlogPostDetailDto?> GetBySlugAsync(string slug)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var row = await conn.QueryFirstOrDefaultAsync<BlogPostDetailRow>($"{SelectDetail} WHERE slug = @slug AND published = true", new { slug });
        return row is null ? null : MapDetail(row);
    }

    public async Task<BlogPostDetailDto?> GetByIdAsync(Guid id)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var row = await conn.QueryFirstOrDefaultAsync<BlogPostDetailRow>($"{SelectDetail} WHERE id = @id", new { id });
        return row is null ? null : MapDetail(row);
    }

    public async Task<BlogPostDto> CreateAsync(CreateBlogPostRequest r)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var id = await conn.ExecuteScalarAsync<Guid>(
            """
            INSERT INTO blog_posts (title, slug, content, excerpt, image_url, category)
            VALUES (@title, @slug, @content, @excerpt, @imageUrl, @category)
            RETURNING id
            """,
            new { title = r.Title, slug = r.Slug, content = r.Content, excerpt = r.Excerpt, imageUrl = r.ImageUrl, category = r.Category });
        var row = await conn.QueryFirstAsync<BlogPostRow>($"{SelectSummary} WHERE id = @id", new { id });
        return MapSummary(row);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateBlogPostRequest r)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var publishedAt = r.Published == true ? "COALESCE(published_at, NOW())" : r.Published == false ? "NULL" : "published_at";
        var affected = await conn.ExecuteAsync(
            $"""
            UPDATE blog_posts SET
                title       = COALESCE(@title, title),
                content     = COALESCE(@content, content),
                excerpt     = COALESCE(@excerpt, excerpt),
                image_url   = COALESCE(@imageUrl, image_url),
                category    = COALESCE(@category, category),
                published   = COALESCE(@published, published),
                published_at = {publishedAt},
                updated_at  = NOW()
            WHERE id = @id
            """,
            new { id, title = r.Title, content = r.Content, excerpt = r.Excerpt, imageUrl = r.ImageUrl, category = r.Category, published = r.Published });
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        var affected = await conn.ExecuteAsync("DELETE FROM blog_posts WHERE id = @id", new { id });
        return affected > 0;
    }

    private static BlogPostDto MapSummary(BlogPostRow r) =>
        new(r.Id, r.Title, r.Slug, r.Excerpt, r.ImageUrl, r.Category, r.PublishedAt);

    private static BlogPostDetailDto MapDetail(BlogPostDetailRow r) =>
        new(r.Id, r.Title, r.Slug, r.Content, r.Excerpt, r.ImageUrl, r.Category, r.PublishedAt);

    private record BlogPostRow(Guid Id, string Title, string Slug, string? Excerpt, string? ImageUrl, string? Category, DateTime? PublishedAt);
    private record BlogPostDetailRow(Guid Id, string Title, string Slug, string? Content, string? Excerpt, string? ImageUrl, string? Category, bool Published, DateTime? PublishedAt);
}
