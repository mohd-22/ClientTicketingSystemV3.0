namespace ClientTicketingSystem.CORE.Dtos.ProductDtos;
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public virtual ICollection<ModuleDto> Modules { get; set; } = new List<ModuleDto>();
}
