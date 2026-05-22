using ClientTicketingSystem.CORE.Models;
namespace SupportHub.DATA.Repositories.Interfaces;
public interface ICommentRepository : IGenericRepository<Comment>
{
    public Task<IEnumerable<Comment>> GetAllCommnets(Guid id);

}