using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;

namespace ClientTicketingSystem.CORE.Specifications;

public class UsersWithFiltersSpecification : BaseSpecification<User>
{
    public UsersWithFiltersSpecification(
        string? search,
        string? sort,
        UserRole? role,
        bool? isActive,
        int pageIndex,
        int pageSize)
        : base(user =>
            (!role.HasValue || user.Role == role.Value) &&
            (!isActive.HasValue || user.IsActive == isActive.Value) &&
            (string.IsNullOrWhiteSpace(search) ||
             user.FullName.ToLower().Contains(search.Trim().ToLower()) ||
             user.UserName.ToLower().Contains(search.Trim().ToLower()) ||
             user.Email.ToLower().Contains(search.Trim().ToLower()) ||
             user.PhoneNumber.Contains(search.Trim())))
    {
        var normalizedSort = sort?.Trim().ToLowerInvariant();

        switch (normalizedSort)
        {
            case "email-asc":
            case "emailasc":
                AddOrderBy(user => user.Email);
                break;
            case "email-desc":
            case "emaildesc":
                AddOrderByDescending(user => user.Email);
                break;
            case "username-asc":
            case "usernameasc":
                AddOrderBy(user => user.UserName);
                break;
            case "username-desc":
            case "usernamedesc":
                AddOrderByDescending(user => user.UserName);
                break;
            case "name-desc":
            case "namedesc":
                AddOrderByDescending(user => user.FullName);
                break;
            case "role-asc":
            case "roleasc":
                AddOrderBy(user => user.Role);
                break;
            case "role-desc":
            case "roledesc":
                AddOrderByDescending(user => user.Role);
                break;
            case "active-asc":
            case "activeasc":
                AddOrderBy(user => user.IsActive);
                break;
            case "active-desc":
            case "activedesc":
                AddOrderByDescending(user => user.IsActive);
                break;
            case "created-desc":
            case "createddesc":
                AddOrderByDescending(user => user.CreatedDate);
                break;
            case "created-asc":
            case "createdasc":
                AddOrderBy(user => user.CreatedDate);
                break;
            case "name-asc":
            case "nameasc":
            default:
                AddOrderBy(user => user.FullName);
                break;
        }

        ApplyPaging(Math.Max(0, (pageIndex - 1) * pageSize), pageSize <= 0 ? 10 : pageSize);
    }
}
