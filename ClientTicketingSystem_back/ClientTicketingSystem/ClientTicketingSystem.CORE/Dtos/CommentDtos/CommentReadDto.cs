namespace ClientTicketingSystem.CORE.Dtos.CommentDtos;
public class CommentReadDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
