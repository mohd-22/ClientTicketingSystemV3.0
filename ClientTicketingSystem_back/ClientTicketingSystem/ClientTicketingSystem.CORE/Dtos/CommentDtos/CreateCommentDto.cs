namespace ClientTicketingSystem.CORE.Dtos.CommentDtos;
public class CreateCommentDto
{
    public string Text { get; set; } = string.Empty;
    public Guid TicketId { get; set; }
}
