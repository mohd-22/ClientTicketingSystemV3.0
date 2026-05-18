using ClientTicketingSystem.CORE.Models;
namespace ClientTicketingSystem.CORE.Specifications;
public class ProductsWithFiltersSpecification : BaseSpecification<Product>
{
    public ProductsWithFiltersSpecification(string? search, string? sort, int pageIndex, int pageSize)
        : base(p =>
            
            (string.IsNullOrWhiteSpace(search) || p.Name.ToLower().Contains(search.Trim().ToLower()) || p.Description.ToLower().Contains(search.Trim().ToLower()))
        )
    {
        var normalizedSort = sort?.Trim().ToLowerInvariant();

        // تطبيق الـ Sorting
        switch (normalizedSort)
        {
            case "description-asc":
            case "descriptionasc":
                AddOrderBy(p => p.Description);
                break;
            case "description-desc":
            case "descriptiondesc":
                AddOrderByDescending(p => p.Description);
                break;
            case "name-desc":
            case "namedesc":
                AddOrderByDescending(p => p.Name);
                break;
            case "modules-asc":
            case "modulesasc":
                AddOrderBy(p => p.ProductModules!.Count());
                break;
            case "modules-desc":
            case "modulesdesc":
                AddOrderByDescending(p => p.ProductModules!.Count());
                break;
            case "name-asc":
            case "nameasc":
                AddOrderBy(p => p.Name);
                break;
            case "created-desc":
            case "createddesc":
                AddOrderByDescending(p => p.CreatedDate);
                break;
            case "created-asc":
            case "createdasc":
                AddOrderBy(p => p.CreatedDate);
                break;
            default:
                AddOrderBy(p => p.Name);
                break;
        }

        ApplyPaging(Math.Max(0, (pageIndex - 1) * pageSize), pageSize <= 0 ? 10 : pageSize);
    }
}
