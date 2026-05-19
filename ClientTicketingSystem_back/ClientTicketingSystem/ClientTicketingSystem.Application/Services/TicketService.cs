using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
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
        var request = await _unitOfWork.Tickets.GetByIdAsync(Id);
        if (request == null) { return new ApiResponse<bool> { Data = false, Message = "Request Not found", Success = false, StatusCode = 404 }; }


        if (request.Status != TicketStatus.New)
        {
            return new ApiResponse<bool> { Data = false, Message = "Request Must Be New To Delete", Success = false, StatusCode = 400 };
        }

        _unitOfWork.Tickets.Delete(request);
        await _unitOfWork.CompleteAsync();
        return new ApiResponse<bool> { Data = true, Message = "Request Deleted Successfully", Success = true, StatusCode = 200 };
    }
    public async Task<ApiResponse<bool>> UpdateTicket(CreateTicketDto dto, Guid TicketId)
    {
        var request = await _unitOfWork.Tickets.GetByIdAsync(TicketId);

        if (request == null) return new ApiResponse<bool> { Data = false, Message = "Ticket Not found", Success = false, StatusCode = 404 };

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
    public async Task<ApiResponse<PaginationDto<TicketDto>>> GetAllTickets(string? search, string? sort, TicketStatus? status, int pageIndex, int pageSize,UserRole role, Guid userId)
    {
        try
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var spec = new TicketsWithFiltersSpecification(search, sort, status, pageIndex, pageSize, role.ToString(), userId);
            var countSpec = new TicketsWithFiltersForCountSpecification(search, status);

            var tickets = await _unitOfWork.Tickets.ListWithSpecAsync(spec);
            var totalCount = await _unitOfWork.Tickets.CountAsync(countSpec);

            var ticketDtos = tickets.Select(ticket => new TicketDto
            {
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


}
