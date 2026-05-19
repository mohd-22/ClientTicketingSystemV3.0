using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientTicketingSystem.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    readonly ITicketService _ticketService;
    readonly ILogger<TicketsController> _logger;
    public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    [Authorize(Roles = $"{nameof(UserRole.Client)}")]
    [HttpPost("CreateRequest")]
    public async Task<ActionResult> CreateRequest(CreateTicketDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            _logger.LogWarning("Invalid user ID format: {UserId}", userId);
            return BadRequest("Invalid user ID.");
        }
        _logger.LogInformation("CreateTicket requested by {UserId} for product {ProductId}", userGuid, dto.ProductId);
        var result = await _ticketService.CreateTicket(dto, userGuid);
        return StatusCode(result.StatusCode, result);
    }
}
