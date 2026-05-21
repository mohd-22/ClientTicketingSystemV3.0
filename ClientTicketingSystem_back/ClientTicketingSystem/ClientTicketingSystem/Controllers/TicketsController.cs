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
    [HttpPost("CreateTicket")]
    public async Task<ActionResult> CreateTicket(CreateTicketDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return BadRequest("Invalid user ID.");
        }
        var result = await _ticketService.CreateTicket(dto, userGuid);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("GetAllTickets")]
    public async Task<ActionResult> GetAllTickets(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] TicketStatus? status,
        [FromQuery] Guid? clientId,
        [FromQuery] Guid? employeeId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10
        )
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return Unauthorized();
        }
        var currentUserRole = Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role)!.Value);
        var result = await _ticketService.GetAllTickets(search, sort, status, pageIndex, pageSize, clientId, employeeId, currentUserRole, userGuid);
        return StatusCode(result.StatusCode, result);
    }
    [HttpGet("GetTicketById/{id}")]
    public async Task<ActionResult> GetTicketById(Guid id)
    {
        var result = await _ticketService.GetTicketById(id);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("UpdateTicket/{id}")] 
    public async Task<ActionResult> UpdateTicket([FromBody] CreateTicketDto dto, Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return Unauthorized("Invalid user ID.");
        }
        var result = await _ticketService.UpdateTicket(dto,id, userGuid);
        return StatusCode(result.StatusCode, result);
    }
}
