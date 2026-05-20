using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.DATA.Data;
using Microsoft.EntityFrameworkCore;
using SupportHub.DATA.Repositories.Interfaces;

namespace SupportHub.DATA.Repositories;
public class TicketsRepository : GenericRepository<Ticket>,ITicketRepository
{
    public TicketsRepository(AppDbContext context) : base(context) { }
    public async Task<Ticket> GetTicketWithDetailsByIdAsync(Guid id)
    {
        return (await _context.Set<Ticket>()
            .Include(t => t.Client)
            .Include(t => t.AssignedUser)
            .Include(t => t.Product)
       .FirstOrDefaultAsync(c => c.Id == id))!;
      
    }
}