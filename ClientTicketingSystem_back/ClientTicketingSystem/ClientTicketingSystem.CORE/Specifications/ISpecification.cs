using System.Linq.Expressions;

namespace ClientTicketingSystem.CORE.Specifications;
public interface ISpecification<T>
{
   
    Expression<Func<T, bool>> Criteria { get; }

   
    Expression<Func<T, object>> OrderBy { get; }
    Expression<Func<T, object>> OrderByDescending { get; }

    List<Expression<Func<T, object>>> Includes { get; }
    int Take { get; }
    int Skip { get; }
    bool IsPagingEnabled { get; }
}
