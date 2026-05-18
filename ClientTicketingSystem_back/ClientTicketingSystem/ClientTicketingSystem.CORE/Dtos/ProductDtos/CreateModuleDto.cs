namespace ClientTicketingSystem.CORE.Dtos.ProductDtos;
public class CreateModuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProdutId { get; set; }
}
