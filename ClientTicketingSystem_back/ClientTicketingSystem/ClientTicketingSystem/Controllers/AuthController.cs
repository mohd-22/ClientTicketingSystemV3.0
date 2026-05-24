using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos.AuthDtos;
using Microsoft.AspNetCore.Mvc;

namespace ClientTicketingSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var result = await _authService.LoginAsync(request);
        return StatusCode(result.StatusCode, result.Data);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(UserRegistraionDto request)
    {
        var result = await _authService.RigisterUserAsync(request);
        return StatusCode(result.StatusCode, result);
    }
}
