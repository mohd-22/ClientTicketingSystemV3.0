using ClientTicketingSystem.CORE.Models;
namespace SupportHub.DATA.Repositories.Interfaces;
public interface ITicketRepository : IGenericRepository<Ticket>
{
    Task<Ticket> GetTicketWithDetailsByIdAsync(Guid id);
}