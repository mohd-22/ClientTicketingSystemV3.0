namespace ClientTicketingSystem.CORE.Models;
public class ProductModule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
}
