using ClientTicketingSystem.CORE.Models;
using ClientTicketingSystem.DATA.Data;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SupportHub.DATA.Repositories.Interfaces;

namespace SupportHub.DATA.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    public IUserRepository Users { get; }
    public IGenericRepository<Attachment> Attachments { get; }
    public ICommentRepository Comments { get; }
    public IProductRepository Products { get; }
    public ITicketRepository Tickets { get; }

    public UnitOfWork(AppDbContext context, ILogger<UnitOfWork> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        Users = new UserRepository(_context);
        Attachments = new GenericRepository<Attachment>(_context);
        Comments = new CommentRepository(_context);
        Products = new ProductRepository(_context, mapper);
        Tickets = new TicketsRepository(_context);
    }

    public async Task<int> CompleteAsync()
    {
        try
        {
            var changes = await _context.SaveChangesAsync();
            if (changes > 0)
                _logger.LogDebug("Saved {ChangeCount} entity changes to the database", changes);
            return changes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save changes to the database");
            throw;
        }
    }
}
