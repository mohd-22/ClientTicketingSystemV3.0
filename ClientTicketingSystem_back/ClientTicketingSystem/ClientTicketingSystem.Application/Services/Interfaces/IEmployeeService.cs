using ClientTicketingSystem.Application.Helpers;

namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface IEmployeeService
{
    Task<ApiResponse<bool>> AssignTicketToEmployee(Guid TicketId, Guid EmployeeId);
}
