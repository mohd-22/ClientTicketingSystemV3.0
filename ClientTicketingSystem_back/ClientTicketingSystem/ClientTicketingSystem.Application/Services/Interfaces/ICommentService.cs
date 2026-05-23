using ClientTicketingSystem.Application.Helpers;
using ClientTicketingSystem.CORE.Dtos.CommentDtos;

namespace ClientTicketingSystem.Application.Services.Interfaces;
public interface ICommentService
{
    Task<ApiResponse<CreateCommentDto>> CreateComment(CreateCommentDto commentDto, Guid Userid);
    Task<ApiResponse<IEnumerable<CommentReadDto>>> GetAllComments(Guid requestId);
}
