using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;

namespace ClientTicketingSystem.CORE.Specifications;

public class TicketsWithFiltersSpecification : BaseSpecification<Ticket>
{
    public TicketsWithFiltersSpecification(
        string? search,
        string? sort,
        TicketStatus? status,
        int pageIndex,
        int pageSize,
        Guid? productId,
        string role,      
        Guid userId       
    )
    : base(ticket =>
        (role == "Manager" ||
         (role == "Employee" && ticket.AssignedEmpId == userId) ||
         (role == "Client" && ticket.ClientId == userId)) &&

        (!status.HasValue || ticket.Status == status.Value) &&
        (!productId.HasValue || ticket.ProductId == productId.Value) &&
        (string.IsNullOrWhiteSpace(search) ||
         ticket.Title.ToLower().Contains(search.Trim().ToLower()) ||
         ticket.Description.ToLower().Contains(search.Trim().ToLower()) ||
         (ticket.Client != null && ticket.Client.FullName.ToLower().Contains(search.Trim().ToLower())) ||
         (ticket.AssignedUser != null && ticket.AssignedUser.FullName.ToLower().Contains(search.Trim().ToLower()))
         ))
    {
        AddInclude(t => t.Client!);
        AddInclude(t => t.AssignedUser!);
        AddInclude(t => t.Product!);

        var normalizedSort = sort?.Trim().ToLowerInvariant();

        switch (normalizedSort)
        {
            case "title-desc":
            case "titledesc":
                AddOrderByDescending(ticket => ticket.Title);
                break;
            case "description-asc":
            case "descriptionasc":
                AddOrderBy(ticket => ticket.Description);
                break;
            case "description-desc":
            case "descriptiondesc":
                AddOrderByDescending(ticket => ticket.Description);
                break;
            case "client-asc":
            case "clientasc":
                AddOrderBy(ticket => ticket.Client != null ? ticket.Client.FullName : string.Empty);
                break;
            case "client-desc":
            case "clientdesc":
                AddOrderByDescending(ticket => ticket.Client != null ? ticket.Client.FullName : string.Empty);
                break;
            case "assigned-asc":
            case "assignedasc":
                AddOrderBy(ticket => ticket.AssignedUser != null ? ticket.AssignedUser.FullName : string.Empty);
                break;
            case "assigned-desc":
            case "assigneddesc":
                AddOrderByDescending(ticket => ticket.AssignedUser != null ? ticket.AssignedUser.FullName : string.Empty);
                break;
            case "created-desc":
            case "createddesc":
                AddOrderByDescending(ticket => ticket.CreatedDate);
                break;
            case "created-asc":
            case "createdasc":
                AddOrderBy(ticket => ticket.CreatedDate);
                break;
            case "title-asc":
            case "titleasc":
            default:
                AddOrderBy(ticket => ticket.Title);
                break;
        }

        ApplyPaging(Math.Max(0, (pageIndex - 1) * pageSize), pageSize <= 0 ? 10 : pageSize);
    }
}