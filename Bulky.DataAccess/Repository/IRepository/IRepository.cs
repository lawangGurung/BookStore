using System.Linq.Expressions;

namespace Bulky.DataAccess;

public interface IRepository<T> where T : class
{
    //T - Catgory
    IEnumerable<T> GetAll(string? includeProperties = null);
    T? Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);
    void Add(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
}
