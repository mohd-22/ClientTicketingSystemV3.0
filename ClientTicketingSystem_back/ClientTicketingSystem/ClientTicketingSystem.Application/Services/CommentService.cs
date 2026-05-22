using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.Application.Services.Interfaces;
using ClientTicketingSystem.CORE.Dtos;
using ClientTicketingSystem.CORE.Models;
using SupportHub.DATA.Repositories.Interfaces;
namespace ClientTicketingSystem.Application.Services;

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommentService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    public async Task<ApiResponse<CreateCommentDto>> CreateComment(CreateCommentDto commentDto, Guid Userid)
    {
        var comment = new Comment
        {
            CommentText = commentDto.Text,
            TicketId = commentDto.TicketId,
            CreatorId = Userid,
            CreatedBy = Userid,
        };
        await _unitOfWork.Comments.AddAsync(comment);
        await _unitOfWork.CompleteAsync();
        return new ApiResponse<CreateCommentDto> { Data = commentDto, Message = "Comment Created Successfully", Success = true, StatusCode = 201 };
    }
    public async Task<ApiResponse<IEnumerable<CommentReadDto>>> GetAllComments(Guid ticketId)
    {
        var comments = await _unitOfWork.Comments.GetAllCommnets(ticketId);
        var commentDtos = comments.Select(c => new CommentReadDto
        {
            Id = c.Id,
            Text = c.CommentText,
            CreatedAt = c.CreatedDate,
            UserName = c.Creator!.FullName,
            UserRole = c.Creator.Role.ToString(),
            UserId = c.CreatorId
        }).ToList();

        return new ApiResponse<IEnumerable<CommentReadDto>> { Data = commentDtos, Message = "Comments Fetched Successfully", Success = true, StatusCode = 200 };
    }
   



}
