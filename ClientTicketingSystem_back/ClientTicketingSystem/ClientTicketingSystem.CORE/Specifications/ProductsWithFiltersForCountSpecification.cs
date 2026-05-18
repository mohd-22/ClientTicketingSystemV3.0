using ClientTicketingSystem.CORE.Models;

namespace ClientTicketingSystem.CORE.Specifications;

public class ProductsWithFiltersForCountSpecification : BaseSpecification<Product>
{
    public ProductsWithFiltersForCountSpecification(string? search)
        : base(p =>
            string.IsNullOrWhiteSpace(search) ||
            p.Name.ToLower().Contains(search.Trim().ToLower()) ||
            p.Description.ToLower().Contains(search.Trim().ToLower()))
    {
    }
}
