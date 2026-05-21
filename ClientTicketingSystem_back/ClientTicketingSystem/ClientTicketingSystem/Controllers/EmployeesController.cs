using ClientTicketingSystem.Application.Services;
using ClientTicketingSystem.Application.Services.Interfaces;
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

    [HttpPut("assign/{ticketId}")]
    public async Task<IActionResult> AssignTicket(Guid ticketId, [FromQuery] Guid employeeId)
    {
        var result = await _employeeService.AssignTicketToEmployee(ticketId, employeeId);
        return StatusCode(result.StatusCode, result);
    }
    [HttpPut("ChangeStatus/{ticketId}")] 
    public async Task<IActionResult> ChangeStatus(Guid ticketId)
    {
        var result = await _employeeService.TicketChangeStatus(ticketId);
        return StatusCode(result.StatusCode, result);
    }
}
