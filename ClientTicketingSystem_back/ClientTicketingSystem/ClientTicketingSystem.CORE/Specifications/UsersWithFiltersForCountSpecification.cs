using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;

namespace ClientTicketingSystem.CORE.Specifications;

public class UsersWithFiltersForCountSpecification : BaseSpecification<User>
{
    public UsersWithFiltersForCountSpecification(string? search, UserRole? role, bool? isActive)
        : base(user =>
            (!role.HasValue || user.Role == role.Value) &&
            (!isActive.HasValue || user.IsActive == isActive.Value) &&
            (string.IsNullOrWhiteSpace(search) ||
             user.FullName.ToLower().Contains(search.Trim().ToLower()) ||
             user.UserName.ToLower().Contains(search.Trim().ToLower()) ||
             user.Email.ToLower().Contains(search.Trim().ToLower()) ||
             user.PhoneNumber.Contains(search.Trim())))
    {
    }
}
