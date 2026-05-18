namespace ClientTicketingSystem.CORE.Models;
public class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public Guid? LastUpdatedBy { get; set; }
}
