using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Models.Enums;
using System.Data;

namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface ITicketService
{
    Task<ApiResponse<CreateTicketDto>> CreateRequest(CreateTicketDto TicketDto);
    Task<ApiResponse<PaginationDto<TicketDto>>> GetAllTickets(string? search, string? sort, TicketStatus? status, int pageIndex, int pageSize,UserRole role, Guid userId);
    Task<ApiResponse<bool>> DeleteRequest(Guid Id);
    Task<ApiResponse<bool>> UpdateRequest(CreateTicketDto dto,Guid TicketId);
}
