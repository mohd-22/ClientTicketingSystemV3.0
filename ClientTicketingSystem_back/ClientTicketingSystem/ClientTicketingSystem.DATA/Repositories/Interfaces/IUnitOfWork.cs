using ClientTicketingSystem.CORE.Models;
namespace SupportHub.DATA.Repositories.Interfaces;
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IGenericRepository<Attachment> Attachments { get; }
    ICommentRepository Comments { get; }
    IProductRepository Products { get; }
    ITicketRepository Tickets { get; }

    Task<int> CompleteAsync();
}

