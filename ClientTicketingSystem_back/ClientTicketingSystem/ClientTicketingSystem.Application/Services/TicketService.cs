using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.TicketDtos;
using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;
using ClientTicketingSystem.CORE.Specifications;
using Microsoft.Extensions.Logging;
using SupportHub.DATA.Repositories.Interfaces;

namespace ClientTicketingSystem.Application.Services;
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<Ticket> _logger;

    public TicketService(IUnitOfWork unitOfWork, ILogger<Ticket> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<CreateTicketDto>> CreateTicket(CreateTicketDto TicketDto,Guid clientId)

    {
        var ticket = new Ticket
        {
            Title = TicketDto.Title,
            Description = TicketDto.Description,
            ProductId = TicketDto.ProductId,
            ClientId = clientId
        };
        _logger.LogInformation("Creating a new ticket with title: {Title}", TicketDto.Title);
        await _unitOfWork.Tickets.AddAsync(ticket);
        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Ticket with title: {Title} created successfully with ID: {Id}", TicketDto.Title, ticket.Id);
        return new ApiResponse<CreateTicketDto> { Data = TicketDto, Message = "Ticket Created Successfully", Success = true, StatusCode = 200 };
    }
    public async Task<ApiResponse<bool>> DeleteTicket(Guid Id)
    {
        _logger.LogInformation("Deleting a ticket with ID: {Id}", Id);
        var request = await _unitOfWork.Tickets.GetByIdAsync(Id);
        if (request == null) { return new ApiResponse<bool> { Data = false, Message = "Request Not found", Success = false, StatusCode = 404 }; }


        if (request.Status != TicketStatus.New)
        {
            return new ApiResponse<bool> { Data = false, Message = "Request Must Be New To Delete", Success = false, StatusCode = 400 };
        }

        _unitOfWork.Tickets.Delete(request);
        await _unitOfWork.CompleteAsync();
        _logger.LogInformation("Request with ID: {Id} deleted successfully", Id);
        return new ApiResponse<bool> { Data = true, Message = "Request Deleted Successfully", Success = true, StatusCode = 200 };
    }
    public async Task<ApiResponse<bool>> UpdateTicket(CreateTicketDto dto, Guid TicketId, Guid clientId)
    {
        var request = await _unitOfWork.Tickets.GetByIdAsync(TicketId);

        if (request == null) return new ApiResponse<bool> { Data = false, Message = "Ticket Not found", Success = false, StatusCode = 404 };
        if (request.ClientId != clientId)
        {
            return new ApiResponse<bool>
            {
                Data = false,
                Message = "You are not authorized to access or modify this ticket.",
                Success = false,
                StatusCode = 403
            };
        }

        if (request.Status != TicketStatus.New)
        {
            return new ApiResponse<bool> { Data = false, Message = "Cannot update ticket once it's processed.", Success = false, StatusCode = 404 };
        }

        request.Title = dto.Title;
        request.Description = dto.Description;
        request.ProductId = dto.ProductId;

        _unitOfWork.Tickets.Update(request);
        await _unitOfWork.CompleteAsync();
        return new ApiResponse<bool> { Data = true, Message = "Ticket Updated Successfully", Success = true, StatusCode = 200 };
    }
    public async Task<ApiResponse<PaginationDto<TicketDto>>> GetAllTickets(
        string? search,
        string? sort,
        TicketStatus? status,
        int pageIndex,
        int pageSize,
        Guid? clientId,
        Guid? employeeId,
        UserRole role,
        Guid userId)
    {
        try
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var spec = new TicketsWithFiltersSpecification(search, sort, status, pageIndex, pageSize, clientId, employeeId, role.ToString(), userId);
            var countSpec = new TicketsWithFiltersForCountSpecification(search, status, clientId, employeeId);

            var tickets = await _unitOfWork.Tickets.ListWithSpecAsync(spec);
            var totalCount = await _unitOfWork.Tickets.CountAsync(countSpec);

            var ticketDtos = tickets.Select(ticket => new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                ClientName = ticket.Client?.FullName ?? string.Empty,
                AssignedEmpName = ticket.AssignedUser?.FullName ?? string.Empty,
                ProductName = ticket.Product?.Name ?? string.Empty,
                Status = ticket.Status.ToString(),
                IsFixed = ticket.IsFixed
            }).ToList();

            var pagedResult = new PaginationDto<TicketDto>(pageIndex, pageSize, totalCount, ticketDtos);

            return new ApiResponse<PaginationDto<TicketDto>>
            {
                Success = true,
                Message = "Tickets retrieved successfully",
                Data = pagedResult,
                StatusCode = 200
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tickets");
            return new ApiResponse<PaginationDto<TicketDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving tickets",
                StatusCode = 500
            };
        }

    }

    public async Task<ApiResponse<TicketDto>> GetTicketById(Guid TicketId, Guid userId)
    {
        var ticket = await _unitOfWork.Tickets.GetTicketWithDetailsByIdAsync(TicketId);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (ticket == null) return new ApiResponse<TicketDto> { Data = null, Message = "Ticket Not found", Success = false, StatusCode = 404 };
        if(ticket.ClientId != userId && user.Role == UserRole.Client) return new ApiResponse<TicketDto> { Data = null, Message = "You are not authorized to access or modify this ticket.", Success = false, StatusCode = 403 };
        var ticketDto = new TicketDto
        {   
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            ClientName = ticket.Client?.FullName ?? string.Empty,
            AssignedEmpName = ticket.AssignedUser?.FullName ?? string.Empty,
            ProductName = ticket.Product?.Name ?? string.Empty,
            Status = ticket.Status.ToString(),
            IsFixed = ticket.IsFixed
        };
        return new ApiResponse<TicketDto> { Data = ticketDto, Message = "Ticket Retrieved Successfully", Success = true, StatusCode = 200 };

    }
    public async Task<ApiResponse<bool>> TicketFixed(Guid TicketId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(TicketId);
        if (ticket == null) return new ApiResponse<bool> { Data = false, Message = "Ticket Not found", Success = false, StatusCode = 404 };
        if (ticket.IsFixed == true) return new ApiResponse<bool> { Data = false, Message = "Ticket Already Fixed", Success = false, StatusCode = 400 };
        if (ticket.Status != TicketStatus.InProgress) return new ApiResponse<bool> { Data = false, Message = "Ticket Must Be InProgress To Be Fixed", Success = false, StatusCode = 400 };
        ticket.IsFixed = true;
        _unitOfWork.Tickets.Update(ticket);
        await _unitOfWork.CompleteAsync();
        return new ApiResponse<bool> { Data = true, Message = "Ticket Fixed Successfully", Success = true, StatusCode = 200 };
    }

}
