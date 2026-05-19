using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.CORE.Models.Enums;

namespace ClientTicketingSystem.CORE.Specifications;

public class TicketsWithFiltersForCountSpecification : BaseSpecification<Ticket>
{
    public TicketsWithFiltersForCountSpecification(string? search, TicketStatus? status)
        : base(ticket =>
            (!status.HasValue || ticket.Status == status.Value) &&
            (string.IsNullOrWhiteSpace(search) ||
             ticket.Title.ToLower().Contains(search.Trim().ToLower()) ||
             ticket.Description.ToLower().Contains(search.Trim().ToLower()) ||
             ticket.Status.ToString().ToLower().Contains(search.Trim().ToLower()) ||
             ticket.IsFixed.ToString().ToLower().Contains(search.Trim().ToLower())))
    {
    }
}