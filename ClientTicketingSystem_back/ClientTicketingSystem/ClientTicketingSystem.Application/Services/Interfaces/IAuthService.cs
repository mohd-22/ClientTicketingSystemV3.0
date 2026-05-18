using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Core.Dtos;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface IAuthService
{
    Task<ApiResponse<string>> LoginAsync(LoginDto request);
    Task<ApiResponse<UserRegistraionDto>> RigisterUserAsync(UserRegistraionDto request);
}