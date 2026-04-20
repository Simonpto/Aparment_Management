using Apartment_Manager.Api.Infrastructure;
using Apartment_Manager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartment_Manager.Api.Controllers;

[ApiController]
[Route("api/apartments")]
public class ApartmentsController(ApartmentRepository repo) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await repo.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var apt = await repo.GetByIdAsync(id);
        return apt is null ? NotFound() : Ok(apt);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] CreateApartmentRequest request)
    {
        var created = await repo.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}"), Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApartmentRequest request)
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

    [HttpGet("{id:guid}/availability")]
    public async Task<IActionResult> GetAvailability(Guid id) =>
        Ok(await repo.GetAvailabilityAsync(id));

    [HttpPut("{id:guid}/availability"), Authorize]
    public async Task<IActionResult> UpsertAvailability(Guid id, [FromBody] UpdateAvailabilityRequest request)
    {
        await repo.UpsertAvailabilityAsync(id, request);
        return NoContent();
    }
}
