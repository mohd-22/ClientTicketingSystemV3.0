using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;

namespace ClientTicketingSystem.CORE.Specifications;

public class TicketsWithFiltersForCountSpecification : BaseSpecification<Ticket>
{
    public TicketsWithFiltersForCountSpecification(
        string? search,
        TicketStatus? status,
        Guid? clientId,
        Guid? employeeId
    )
    : base(ticket =>
        (!status.HasValue || ticket.Status == status.Value) &&
        (!clientId.HasValue || ticket.ClientId == clientId.Value) &&
        (!employeeId.HasValue || ticket.AssignedEmpId == employeeId.Value) &&
        (string.IsNullOrWhiteSpace(search) ||
         ticket.Title.ToLower().Contains(search.Trim().ToLower()) ||
         ticket.Description.ToLower().Contains(search.Trim().ToLower()) ||
         (ticket.Client != null && ticket.Client.FullName.ToLower().Contains(search.Trim().ToLower())) ||
         (ticket.AssignedUser != null && ticket.AssignedUser.FullName.ToLower().Contains(search.Trim().ToLower()))))
    {
    }
}
