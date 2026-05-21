using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Models.Enums;
using Microsoft.Extensions.Logging;
using SupportHub.DATA.Repositories.Interfaces;

namespace ClientTicketingSystem.Application.Services;
public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeService> _logger;
    public EmployeeService(IUnitOfWork unitOfWork, ILogger<EmployeeService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<ApiResponse<bool>> AssignTicketToEmployee(Guid TicketId, Guid EmployeeId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(TicketId);
        if (ticket == null) return new ApiResponse<bool> { Data = false, Message = "Ticket Not found", Success = false, StatusCode = 404 };

        var employee = await _unitOfWork.Users.GetByIdAsync(EmployeeId);
        if (employee == null) return new ApiResponse<bool> { Data = false, Message = "Employee Not found", Success = false, StatusCode = 404 };

        if (!employee.IsActive) return new ApiResponse<bool> { Data = false, Message = "Employee is not active", Success = false, StatusCode = 400 };

        if (ticket.AssignedEmpId != null) return new ApiResponse<bool> { Data = false, Message = "Ticket already assigned to an employee", Success = false, StatusCode = 400 };

        if (ticket.Status == TicketStatus.New || ticket.Status == TicketStatus.Paused)
        {
            ticket.Status = TicketStatus.Assigned;
            ticket.AssignedEmpId = EmployeeId;
            _unitOfWork.Tickets.Update(ticket);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse<bool> { Data = true, Message = "Ticket Assigned Successfully", Success = true, StatusCode = 200 };
        }
        return new ApiResponse<bool> { Data = false, Message = "you cant assign staff to a request unless it 'New' or 'Paused' ", Success = false, StatusCode = 400 };
    }
    public async Task<ApiResponse<bool>> TicketChangeStatus(Guid TicketId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(TicketId);
        if (ticket == null) return new ApiResponse<bool> { Data = false, Message = "Ticket Not found", Success = false, StatusCode = 404 };
        if(ticket.Status == TicketStatus.Assigned)
        {
            ticket.Status = TicketStatus.InProgress;
            _unitOfWork.Tickets.Update(ticket);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse<bool> { Data = true, Message = "Ticket Updated Successfully", Success = true, StatusCode = 200 };
        }
        else if(ticket.Status == TicketStatus.InProgress)
        {
            if(ticket.IsFixed == false) return new ApiResponse<bool> { Data = false, Message = "Ticket is not fixed yet", Success = false, StatusCode = 400 };
            ticket.Status = TicketStatus.Closed;
            _unitOfWork.Tickets.Update(ticket);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse<bool> { Data = true, Message = "Ticket Updated Successfully", Success = true, StatusCode = 200 };
        }
        return new ApiResponse<bool> { Data = false, Message = "you cant change the status of a request unless it 'Assigned' or 'InProgress' ", Success = false, StatusCode = 400 };
    }

}
