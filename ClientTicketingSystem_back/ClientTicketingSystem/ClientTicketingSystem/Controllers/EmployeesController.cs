using ClientTicketingSystem.Application.Services;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientTicketingSystem.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [Authorize(Roles = nameof(UserRole.Manager))]
    [HttpPut("assign/{ticketId}")]
    public async Task<IActionResult> AssignTicket(Guid ticketId, [FromQuery] Guid employeeId)
    {
        var result = await _employeeService.AssignTicketToEmployee(ticketId, employeeId);
        return StatusCode(result.StatusCode, result);
    }
    [Authorize(Roles = nameof(UserRole.Employee))]
    [HttpPut("ChangeStatus/{ticketId}")] 
    public async Task<IActionResult> ChangeStatus(Guid ticketId)
    {
        var result = await _employeeService.TicketChangeStatus(ticketId);
        return StatusCode(result.StatusCode, result);
    }
}
