using ClientTicketingSystem.CORE.Models;
namespace SupportHub.DATA.Repositories.Interfaces;
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IGenericRepository<Attachment> Attachments { get; }
    ICommentRepository Comments { get; }
    IGenericRepository<ProductModule> ProdectModules { get; }
    IProductRepository Products { get; }
    ITicketRepository Tickets { get; }

    Task<int> CompleteAsync();
}

