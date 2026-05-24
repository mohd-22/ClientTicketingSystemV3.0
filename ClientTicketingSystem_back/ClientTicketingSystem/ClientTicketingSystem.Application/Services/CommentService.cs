using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos.CommentDtos;
using ClientTicketingSystem.CORE.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SupportHub.DATA.Repositories.Interfaces;
namespace ClientTicketingSystem.Application.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommentService> _logger;
    private readonly IMapper _mapper;


    public CommentService(IUnitOfWork unitOfWork, ILogger<CommentService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }
    public async Task<ApiResponse<IEnumerable<CommentReadDto>>> GetAllComments(Guid ticketId)
    {
        try
        {
            _logger.LogInformation("Getting Comments");
            var comments = await _unitOfWork.Comments.GetAllCommnets(ticketId);
            var commentDtos = _mapper.Map<List<CommentReadDto>>(comments);
            _logger.LogInformation("Comments Fetched Successfully");
            return new ApiResponse<IEnumerable<CommentReadDto>> { Data = commentDtos, Message = "Comments Fetched Successfully", Success = true, StatusCode = 200 };
        }
        catch(Exception ex) { 
            _logger.LogError(ex, "Error getting Comments");
            return new ApiResponse<IEnumerable<CommentReadDto>> { Data = null, Message = ex.Message, Success = false, StatusCode = 500 };
        }
    }
    public async Task<ApiResponse<CreateCommentDto>> CreateComment(CreateCommentDto commentDto, Guid Userid)
    {
        try
        {
            _logger.LogInformation("Creating Comment");
            var comment = new Comment
            {
                CommentText = commentDto.Text,
                TicketId = commentDto.TicketId,
                CreatorId = Userid,
                CreatedBy = Userid,
                CreatedDate = DateTime.Now
            };
            _logger.LogInformation("Comment Created");
            await _unitOfWork.Comments.AddAsync(comment);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse<CreateCommentDto> { Data = commentDto, Message = "Comment Created Successfully", Success = true, StatusCode = 201 };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Comment");
            return new ApiResponse<CreateCommentDto> { Data = commentDto, Message = ex.Message, Success = false, StatusCode = 500 };
        }
    }
}
