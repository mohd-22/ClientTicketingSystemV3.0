using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientTicketingSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [Authorize(Roles = $"{nameof(UserRole.Client)},{nameof(UserRole.Employee)}")]
        [HttpPost]
        public async Task<ActionResult> CreateComment(CreateCommentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out Guid userGuid))
            {
                return Unauthorized("User ID is not valid.");
            }
            var result = await _commentService.CreateComment(dto, userGuid);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpGet("{requestId}")]
        public async Task<ActionResult> GetAllComments(Guid requestId)
        {

            var result = await _commentService.GetAllComments(requestId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
