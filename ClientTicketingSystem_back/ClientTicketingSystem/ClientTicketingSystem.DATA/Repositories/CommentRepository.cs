using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.DATA.Data;
using Microsoft.EntityFrameworkCore;
using SupportHub.DATA.Repositories.Interfaces;
namespace SupportHub.DATA.Repositories;
public class CommentRepository : GenericRepository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context) { }
    public async Task<IEnumerable<Comment>> GetAllCommnets(Guid id)
    {
        return await _context.Set<Comment>()
        .Include(c => c.Creator)
        .Include(c => c.Ticket)
        .Where(c => c.TicketId == id).ToListAsync();
    }

}