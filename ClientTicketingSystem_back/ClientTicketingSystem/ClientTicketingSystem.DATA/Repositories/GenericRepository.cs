using ClientTicketingSystem.CORE.Specifications;
using ClientTicketingSystem.DATA.Data;
using ClientTicketingSystem.DATA.Specification;
using Microsoft.EntityFrameworkCore;
using SupportHub.DATA.Repositories.Interfaces;
using System.Linq.Expressions;

namespace SupportHub.DATA.Repositories;
public class GenericRepository <T>: IGenericRepository <T> where T : class
{
    protected AppDbContext _context;

    public async Task<IEnumerable<T>> FindAsNoTrackingAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }
    public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> match)
    {
        return await _context.Set<T>()
            .Where(match)
            .ToListAsync();
    }
    public async Task<int> CountAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).CountAsync();
    }
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _context.Set<T>().AnyAsync(predicate);
    }
    public async Task<T> FindAsync(Expression<Func<T, bool>> match)
    {
        return (await _context.Set<T>().FirstOrDefaultAsync(match))!;
    }
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
    public GenericRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<T> GetByIdAsync(object id)
    {
        return (await _context.Set<T>().FindAsync(id))!;
    }
    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }
    public T Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
        return entity;
    }
    public T Update(T entity)
    {
        
        _context.Set<T>().Update(entity);
        return entity;
    }

    public async Task<IReadOnlyList<T>> ListWithSpecAsync(ISpecification<T> spec)
    {
        return await ApplySpecification(spec).ToListAsync();
    }

    private IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
    }
}
