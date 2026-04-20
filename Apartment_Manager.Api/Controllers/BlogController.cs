using Apartment_Manager.Api.Infrastructure;
using Apartment_Manager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartment_Manager.Api.Controllers;

[ApiController]
[Route("api/blog")]
public class BlogController(BlogRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await repo.GetAllPublishedAsync());

    [HttpGet("all"), Authorize]
    public async Task<IActionResult> GetAllAdmin() => Ok(await repo.GetAllAsync());

    [HttpGet("{id:guid}"), Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        var post = await repo.GetByIdAsync(id);
        return post is null ? NotFound() : Ok(post);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var post = await repo.GetBySlugAsync(slug);
        return post is null ? NotFound() : Ok(post);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostRequest request)
    {
        var created = await repo.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}"), Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBlogPostRequest request)
    {
        var ok = await repo.UpdateAsync(id, request);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}"), Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await repo.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
