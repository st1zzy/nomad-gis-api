using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Interfaces;

namespace nomad_gis_V2.Controllers
{
    [ApiController]
    [Route("api/v1/points")]
    [Authorize]
    public class MapPointsController : ControllerBase
    {
        private readonly IMapPointService _mapPointService;

        public MapPointsController(IMapPointService mapPointService)
        {
            _mapPointService = mapPointService;
        }

        // GET: api/MapPoints
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var points = await _mapPointService.GetAllAsync();
            return Ok(points);
        }

        // GET: api/MapPoints/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var point = await _mapPointService.GetByIdAsync(id);
            if (point == null) return NotFound();
            return Ok(point);
        }

        // POST: api/MapPoints
        [HttpPost]
        [Authorize(Roles = "Admin")] // <-- ДОБАВЛЕНО
        public async Task<IActionResult> Create([FromBody] MapPointCreateRequest dto)
        {
            var created = await _mapPointService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/MapPoints/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // <-- ДОБАВЛЕНО
        public async Task<IActionResult> Update(Guid id, [FromBody] MapPointUpdateRequest dto)
        {
            var updated = await _mapPointService.UpdateAsync(id, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE: api/MapPoints/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // <-- ДОБАВЛЕНО
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _mapPointService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}