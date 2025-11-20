using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nomad_gis_V2.DTOs.Messages;
using nomad_gis_V2.Exceptions;
using nomad_gis_V2.Interfaces;
using System.Security.Claims;

namespace nomad_gis_V2.Controllers
{
    [ApiController]
    [Route("api/v1/messages")]
    [Authorize]
    public class MesseagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MesseagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet("point/{pointId}")]
        public async Task<IActionResult> GetMessagesByPointId(Guid pointId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var messages = await _messageService.GetMessagesByPointIdAsync(pointId, userId);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage([FromBody] MessageRequest dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _messageService.CreateMessageAsync(userId, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var success = await _messageService.DeleteMessageAsync(id, userId);
            
            if (!success) 
                throw new NotFoundException("Message not found or you don't have permission to delete it.");

            return NoContent();
        }

        // --- НОВЫЙ МЕТОД ДЛЯ АДМИНА ---
        [HttpDelete("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteMessage(Guid id)
        {
            // Этот метод удаляет сообщение, не проверяя, кто автор
            var success = await _messageService.AdminDeleteMessageAsync(id);

            if (!success)
                throw new NotFoundException("Message not found.");

            return Ok(new { message = "Message deleted by admin" });
        }
        
        [HttpPost("{id}/like")]
        public async Task<IActionResult> LikeMessage(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _messageService.ToggleLikeAsync(id, userId);
            
            return Ok(result);
        }
    }
}