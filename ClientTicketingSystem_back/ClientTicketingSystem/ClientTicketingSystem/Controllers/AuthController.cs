using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.Core.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ClientTicketingSystem.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        _logger.LogInformation("Login attempt for {LoginIdentifier}", request.EmailOrUsername);

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            _logger.LogWarning("Login failed for {LoginIdentifier}. Status: {StatusCode}, Message: {Message}",
                request.EmailOrUsername, result.StatusCode, result.Message);
        else
            _logger.LogInformation("Login succeeded for {LoginIdentifier}", request.EmailOrUsername);

        return StatusCode(result.StatusCode, result.Data);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(UserRegistraionDto request)
    {
        _logger.LogInformation("Registration attempt for username {UserName}", request.UserName);

        var result = await _authService.RigisterUserAsync(request);

        if (!result.Success)
            _logger.LogWarning("Registration failed for {UserName}. Status: {StatusCode}, Message: {Message}",
                request.UserName, result.StatusCode, result.Message);
        else
            _logger.LogInformation("User registered successfully: {UserName}", request.UserName);

        return StatusCode(result.StatusCode, result);
    }
}
