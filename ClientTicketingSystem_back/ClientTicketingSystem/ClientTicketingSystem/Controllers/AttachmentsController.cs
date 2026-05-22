using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientTicketingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Manager))]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;
        public AttachmentController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpPost("UploadAttachment/{TicketId}")]
        public async Task<IActionResult> UploadAttachment([FromRoute] Guid TicketId, IFormFile file)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return Unauthorized("User ID is not valid.");
            }
            var result = await _attachmentService.UploadAttachment(userGuid, TicketId, file);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("Ticket/{TicketId}")]
        public async Task<IActionResult> GetAttachmentsByTicket([FromRoute] Guid TicketId)
        {
            var result = await _attachmentService.GetAttachmentsByTicket(TicketId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            var (fileBytes, contentType, fileName) = await _attachmentService.DownloadFile(id);

            if (fileBytes == null)
            {
                return NotFound("File Not Found");
            }

            return File(fileBytes, contentType, fileName);
        }
    }
}
