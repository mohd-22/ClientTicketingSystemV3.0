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
            var countSpec = new TicketsWithFiltersForCountSpecification(search, status, clientId, employeeId, role.ToString(), userId);

            var tickets = await _unitOfWork.Tickets.ListWithSpecAsync(spec);
            var totalCount = await _unitOfWork.Tickets.CountAsync(countSpec);

            /*
            // Previous manual mapping (kept here as a commented reference):
            var ticketDtos = tickets.Select(t => new TicketDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                IsFixed = t.IsFixed,
                Product = t.Product == null ? null : new ProductDto { Id = t.Product.Id, Name = t.Product.Name },
                Client = t.Client == null ? null : new UserDto { Id = t.Client.Id, FullName = t.Client.FullName, ImageUrl = t.Client.ImageUrl },
                AssignedEmployeeId = t.AssignedEmpId,
                CreatedDate = t.CreatedDate
            }).ToList();
            */

            var ticketDtos = _mapper.Map<List<TicketDto>>(tickets);

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

        /*
        // Previous manual mapping for ticket details (kept commented):
        var ticketDto = new TicketDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.ToString(),
            IsFixed = ticket.IsFixed,
            Product = ticket.Product == null ? null : new ProductDto { Id = ticket.Product.Id, Name = ticket.Product.Name },
            Client = ticket.Client == null ? null : new UserDto { Id = ticket.Client.Id, FullName = ticket.Client.FullName, ImageUrl = ticket.Client.ImageUrl },
            AssignedEmployeeId = ticket.AssignedEmpId,
            Comments = ticket.Comments?.Select(c => new CommentReadDto { Id = c.Id, Text = c.CommentText, CreatorId = c.CreatorId, CreatedDate = c.CreatedDate }).ToList(),
            Attachments = ticket.Attachments?.Select(a => new AttachmentDto { Id = a.Id, FileName = a.FileName, FilePath = a.FilePath }).ToList(),
            CreatedDate = ticket.CreatedDate
        };
        */

        var ticketDto = _mapper.Map<TicketDto>(ticket);
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
