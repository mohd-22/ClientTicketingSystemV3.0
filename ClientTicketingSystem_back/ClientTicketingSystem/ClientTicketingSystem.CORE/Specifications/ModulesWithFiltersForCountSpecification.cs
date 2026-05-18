using ClientTicketingSystem.CORE.Models;

namespace ClientTicketingSystem.CORE.Specifications;

public class ModulesWithFiltersForCountSpecification : BaseSpecification<ProductModule>
{
    public ModulesWithFiltersForCountSpecification(string? search, Guid? productId)
        : base(module =>
            (!productId.HasValue || module.ProductId == productId.Value) &&
            (string.IsNullOrWhiteSpace(search) ||
             module.Name.ToLower().Contains(search.Trim().ToLower()) ||
             module.Description.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
