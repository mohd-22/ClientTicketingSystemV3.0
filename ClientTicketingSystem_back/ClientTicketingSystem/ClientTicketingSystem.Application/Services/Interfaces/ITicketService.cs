using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Models.Enums;
using System.Data;

namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface ITicketService
{
    Task<ApiResponse<CreateTicketDto>> CreateTicket(CreateTicketDto TicketDto,Guid clientId);
    Task<ApiResponse<PaginationDto<TicketDto>>> GetAllTickets(string? search,
                                                              string? sort,
                                                              TicketStatus? status,
                                                              int pageIndex,
                                                              int pageSize,
                                                              Guid? clientId,
                                                              Guid? employeeId,
                                                              UserRole role,
                                                              Guid userId);
    Task<ApiResponse<bool>> UpdateTicket(CreateTicketDto dto,Guid TicketId,Guid userId);
    Task<ApiResponse<TicketDto>> GetTicketById(Guid TicketId);
    Task<ApiResponse<bool>> DeleteTicket(Guid Id);
}
