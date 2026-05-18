namespace ClientTicketingSystem.CORE.Dtos.ProductDtos
{
    public class ProductWithCountDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ModulesCount { get; set; }
    }
}
