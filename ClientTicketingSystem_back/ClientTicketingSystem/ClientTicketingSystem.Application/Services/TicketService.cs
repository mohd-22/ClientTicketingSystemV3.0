using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Dtos.TicketDtos;
using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;
using ClientTicketingSystem.CORE.Specifications;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SupportHub.DATA.Repositories.Interfaces;

namespace ClientTicketingSystem.Application.Services;
public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TicketService> _logger;
    private readonly IMapper _mapper;

    public TicketService(IUnitOfWork unitOfWork, ILogger<TicketService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
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
        _logger.LogInformation("Ticket fixed successfully.");
        return new ApiResponse<bool> { Data = true, Message = "Ticket Fixed Successfully", Success = true, StatusCode = 200 };
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
            _logger.LogInformation("Retrieving tickets");
            pageIndex = Math.Max(1, pageIndex);
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var spec = new TicketsWithFiltersSpecification(search, sort, status, pageIndex, pageSize, clientId, employeeId, role.ToString(), userId);
            var countSpec = new TicketsWithFiltersForCountSpecification(search, status, clientId, employeeId, role.ToString(), userId);

            var tickets = await _unitOfWork.Tickets.ListWithSpecAsync(spec);
            var totalCount = await _unitOfWork.Tickets.CountAsync(countSpec);          

            var ticketDtos = _mapper.Map<List<TicketDto>>(tickets);

            var pagedResult = new PaginationDto<TicketDto>(pageIndex, pageSize, totalCount, ticketDtos);

            _logger.LogInformation("Retrieved {TicketCount} of {TotalCount} tickets (page {PageIndex}, size {PageSize})", tickets.Count, totalCount, pageIndex, pageSize);
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
        _logger.LogInformation("Retrieving ticket with ID: {TicketId}", TicketId);
        var ticket = await _unitOfWork.Tickets.GetTicketWithDetailsByIdAsync(TicketId);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (ticket == null) return new ApiResponse<TicketDto> { Data = null, Message = "Ticket Not found", Success = false, StatusCode = 404 };
        if(ticket.ClientId != userId && user.Role == UserRole.Client) return new ApiResponse<TicketDto> { Data = null, Message = "You are not authorized to access or modify this ticket.", Success = false, StatusCode = 403 };

        var ticketDto = _mapper.Map<TicketDto>(ticket);
        _logger.LogInformation("Retrieved ticket with ID: {TicketId}", TicketId);
        return new ApiResponse<TicketDto> { Data = ticketDto, Message = "Ticket Retrieved Successfully", Success = true, StatusCode = 200 };

    }
    public async Task<ApiResponse<bool>> UpdateTicket(CreateTicketDto dto, Guid TicketId, Guid clientId)
    {
        var request = await _unitOfWork.Tickets.GetByIdAsync(TicketId);
        if (request == null) return new ApiResponse<bool> { Data = false, Message = "Ticket Not found", Success = false, StatusCode = 404 };
        _logger.LogInformation("Updating ticket with ID: {TicketId}", TicketId);
        if (request.ClientId != clientId)
        {
            _logger.LogWarning("You are not authorized to access or modify this ticket.");
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
        _logger.LogInformation("Ticket with ID: {TicketId} updated successfully", TicketId);
        return new ApiResponse<bool> { Data = true, Message = "Ticket Updated Successfully", Success = true, StatusCode = 200 };
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
}
