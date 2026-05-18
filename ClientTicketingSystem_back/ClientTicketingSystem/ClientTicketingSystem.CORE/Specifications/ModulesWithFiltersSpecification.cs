using ClientTicketingSystem.CORE.Models;

namespace ClientTicketingSystem.CORE.Specifications;

public class ModulesWithFiltersSpecification : BaseSpecification<ProductModule>
{
    public ModulesWithFiltersSpecification(string? search, string? sort, Guid? productId, int pageIndex, int pageSize)
        : base(module =>
            (!productId.HasValue || module.ProductId == productId.Value) &&
            (string.IsNullOrWhiteSpace(search) ||
            module.Name.ToLower().Contains(search.Trim().ToLower()) ||
            module.Description.ToLower().Contains(search.Trim().ToLower())))
    {
        var normalizedSort = sort?.Trim().ToLowerInvariant();

        switch (normalizedSort)
        {
            case "description-asc":
            case "descriptionasc":
                AddOrderBy(module => module.Description);
                break;
            case "description-desc":
            case "descriptiondesc":
                AddOrderByDescending(module => module.Description);
                break;
            case "name-desc":
            case "namedesc":
                AddOrderByDescending(module => module.Name);
                break;
            case "name-asc":
            case "nameasc":
                AddOrderBy(module => module.Name);
                break;
            case "created-desc":
            case "createddesc":
                AddOrderByDescending(module => module.CreatedDate);
                break;
            case "created-asc":
            case "createdasc":
                AddOrderBy(module => module.CreatedDate);
                break;
            case "productid-asc":
            case "productidasc":
                AddOrderBy(module => module.ProductId);
                break;
            case "productid-desc":
            case "productiddesc":
                AddOrderByDescending(module => module.ProductId);
                break;
            default:
                AddOrderBy(module => module.Name);
                break;
        }

        ApplyPaging(Math.Max(0, (pageIndex - 1) * pageSize), pageSize <= 0 ? 10 : pageSize);
    }
}