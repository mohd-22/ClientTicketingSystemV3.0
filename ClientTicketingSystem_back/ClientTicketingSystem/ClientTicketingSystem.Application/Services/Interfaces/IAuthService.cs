using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.CORE.Dtos.AuthDtos;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface IAuthService
{
    Task<ApiResponse<string>> LoginAsync(LoginDto request);
    Task<ApiResponse<UserRegistraionDto>> RigisterUserAsync(UserRegistraionDto request);
}