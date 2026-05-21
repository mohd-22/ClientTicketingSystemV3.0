using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.Core.Dtos;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClientTicketingSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = nameof(UserRole.Manager))]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }


    [Authorize(Roles = $"{nameof(UserRole.Manager)},{nameof(UserRole.Employee)}")]
    [HttpGet("GetAllUsers")]
    public async Task<ActionResult> GetAllUsers(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] UserRole? role,
        [FromQuery] bool? isActive,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 4)
    {
        var result = await _userService.GetAllUsersAsync(search, sort, role, isActive, pageIndex, pageSize);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = nameof(UserRole.Manager))]
    [HttpGet("GetUserById/{id}")]
    public async Task<ActionResult> GetUserById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = nameof(UserRole.Manager))]
    [HttpPost("Activate/{id}")]
    public async Task<ActionResult> ActivateUser(Guid id)
    {
        var result = await _userService.ActivateUserAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize(Roles = nameof(UserRole.Manager))]
    [HttpPost("Deactivate/{id}")]
    public async Task<ActionResult> DeactivateUser(Guid id)
    {
        var result = await _userService.DeactivateUserAsync(id);
        return StatusCode(result.StatusCode, result);
    }
    [Authorize(Roles = nameof(UserRole.Manager))]
    [HttpPost("AddEmployee")]   
    public async Task<ActionResult> AddEmployee(UserRegistraionDto userRegistraionDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userId, out Guid userGuid))
        {
            return Unauthorized();
        }
        var result = await _userService.CreateUserAsync(userRegistraionDto, userGuid);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}")] 
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdtaeUserAsync(dto,id);
        return StatusCode(result.StatusCode, result);
    }

}
