using ClientTicketingSystem.CORE.Specifications;
namespace ClientTicketingSystem.DATA.Specification;
public class SpecificationEvaluator<TEntity> where TEntity : class
{
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
    {
        var query = inputQuery;

        // 1. تطبيق الفلترة والبحث (Where)
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        // 2. تطبيق الترتيب التصاعدي (Order By)
        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        // أو التنازلي (Order By Descending)
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // 3. تطبيق الـ Paging (يجب أن يكون دائماً آخر خطوة بعد الترتيب)
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        return query;
    }
}
