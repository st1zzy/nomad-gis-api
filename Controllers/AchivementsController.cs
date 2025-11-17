using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.Interfaces;

namespace nomad_gis_V2.Controllers;

[ApiController]
[Route("api/v1/achievements")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementService _service;

    public AchievementsController(IAchievementService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<AchievementResponse>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<AchievementResponse>> Get(Guid id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AchievementResponse>> Create([FromForm] AchievementCreateRequest request)
    {
        var achievement = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(Get), new { id = achievement.Id }, achievement);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AchievementResponse>> Update(Guid id, [FromForm] AchievementUpdateRequest request)
    {
        return Ok(await _service.UpdateAsync(id, request));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        bool deleted = await _service.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}