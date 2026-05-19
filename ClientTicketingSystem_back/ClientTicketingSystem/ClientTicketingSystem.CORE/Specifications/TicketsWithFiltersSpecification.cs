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
        string role,      
        Guid userId       
    )
    : base(ticket =>
        (role == "Manager" ||
         (role == "Employee" && ticket.AssignedEmpId == userId) ||
         (role == "Client" && ticket.ClientId == userId)) &&

        (!status.HasValue || ticket.Status == status.Value) &&
        (string.IsNullOrWhiteSpace(search) ||
         ticket.Title.ToLower().Contains(search.Trim().ToLower()) ||
         ticket.Description.ToLower().Contains(search.Trim().ToLower())
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
            case "status-asc":
            case "statusasc":
                AddOrderBy(ticket => ticket.Status);
                break;
            case "status-desc":
            case "statusdesc":
                AddOrderByDescending(ticket => ticket.Status);
                break;
            case "fixed-asc":
            case "fixedasc":
                AddOrderBy(ticket => ticket.IsFixed);
                break;
            case "fixed-desc":
            case "fixeddesc":
                AddOrderByDescending(ticket => ticket.IsFixed);
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