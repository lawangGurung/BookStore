using Bulky.Models;

namespace Bulky.DataAccess;

public interface ICategoryRepository : IRepository<Category>
{
    void Update(Category obj);

}
