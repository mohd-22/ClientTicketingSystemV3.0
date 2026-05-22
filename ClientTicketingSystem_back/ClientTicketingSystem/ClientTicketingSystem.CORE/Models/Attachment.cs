namespace ClientTicketingSystem.CORE.Models;
public class Attachment : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public Guid TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    
}
