namespace ClientTicketingSystem.CORE.Models;
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<ProductModule>? ProductModules { get; set; }
    public ICollection<Ticket>? Tickets { get; set; }
}
