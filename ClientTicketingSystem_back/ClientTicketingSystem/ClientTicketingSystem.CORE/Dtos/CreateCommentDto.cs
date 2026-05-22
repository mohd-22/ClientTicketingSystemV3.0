namespace ClientTicketingSystem.CORE.Dtos;
public class CreateCommentDto
{
    public string Text { get; set; } = string.Empty;
    public Guid TicketId { get; set; }
}
