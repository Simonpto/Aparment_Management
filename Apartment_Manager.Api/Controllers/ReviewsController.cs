using Apartment_Manager.Api.Services;
using Apartment_Manager.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartment_Manager.Api.Controllers;

[ApiController]
[Route("api")]
public class ReviewsController(IReviewService service) : ControllerBase
{
    [HttpGet("apartments/{apartmentId:guid}/reviews")]
    public async Task<IActionResult> GetForApartment(Guid apartmentId) =>
        Ok(await service.GetApprovedByApartmentAsync(apartmentId));

    [HttpGet("reviews/admin"), Authorize]
    public async Task<IActionResult> GetAll() => Ok(await service.GetAllAsync());

    [HttpPost("reviews")]
    public async Task<IActionResult> Submit([FromBody] CreateReviewRequest request)
    {
        try
        {
            var review = await service.SubmitAsync(request);
            return CreatedAtAction(null, review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("reviews/{id:guid}/approve"), Authorize]
    public async Task<IActionResult> Approve(Guid id)
    {
        var ok = await service.ApproveAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("reviews/{id:guid}"), Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await service.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}
