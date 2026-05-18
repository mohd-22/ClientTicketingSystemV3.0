using ClientTicketingSystem.CORE.Specifications;
using System.Linq.Expressions;
namespace SupportHub.DATA.Repositories.Interfaces;
public interface IGenericRepository <T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> FindAsync(Expression<Func<T, bool>> match);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> match);
    Task<T> GetByIdAsync(object id);
    Task<T> AddAsync(T entity);
    T Delete(T entity);
    T Update(T entity);
    Task<IEnumerable<T>> FindAsNoTrackingAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    Task<IReadOnlyList<T>> ListWithSpecAsync(ISpecification<T> spec);
    Task<int> CountAsync(ISpecification<T> spec);


}
