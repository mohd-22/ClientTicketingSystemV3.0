
using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.AuthDtos;
using ClientTicketingSystem.CORE.Dtos.UserDtos;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface IUserService
{
    Task<ApiResponse<UserRegistraionDto>> CreateUserAsync(UserRegistraionDto request,Guid UserId);
    Task<ApiResponse<PaginationDto<UserDto>>> GetAllUsersAsync(string? search,
                                                               string? sort,
                                                               UserRole? role,
                                                               bool? isActive,
                                                               int pageIndex,
                                                               int pageSize);
    Task<ApiResponse<bool>> UpdtaeUserAsync(UpdateUserDto request,Guid id);
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id);
    Task<ApiResponse<bool>> DeactivateUserAsync(Guid id);
    Task<ApiResponse<bool>> ActivateUserAsync(Guid id);
    Task<ApiResponse<bool>> ChangeAvatar(Guid userId, IFormFile file);
    Task<ApiResponse<bool>> AssignTicketToEmployee(Guid TicketId, Guid EmployeeId);
    Task<ApiResponse<bool>> TicketChangeStatus(Guid TicketId);

}
