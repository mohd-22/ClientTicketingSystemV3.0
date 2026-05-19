using ClientTicketingSystem.CORE.Models.Enums;

namespace ClientTicketingSystem.CORE.Models;
public class Ticket : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public User? Client { get; set; }
    public Guid? AssignedEmpId { get; set; }
    public User? AssignedUser { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public bool IsFixed { get; set; } = false;
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public ICollection<Comment>? Comments { get; set; }
    public ICollection<Attachment>? Attachments { get; set; }
}
