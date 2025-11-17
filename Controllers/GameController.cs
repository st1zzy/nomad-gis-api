using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Game;
using nomad_gis_V2.Interfaces;
using System.Security.Claims;

namespace nomad_gis_V2.Controllers
{
    [ApiController]
    [Route("api/v1/game")]
    [Authorize] // Доступно только авторизованным
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("check-location")]
        public async Task<IActionResult> CheckLocation([FromBody] CheckLocationRequest request)
        {
            // Берем ID пользователя из его JWT-токена
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var result = await _gameService.CheckAndUnlockPointsAsync(userId, request);

            if (!result.Success)
            {
                return Ok(result); // Все равно Ok, просто с сообщением "ничего не найдено"
            }

            return Ok(result); // Возвращаем DTO с успехом
        }
    }
}