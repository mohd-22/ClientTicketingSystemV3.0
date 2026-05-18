namespace ClientTicketingSystem.CORE.Models;
public class Comment : BaseEntity
{
    public string CommentText { get; set; } = string.Empty;
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public User? Creator { get; set; }
}
