using Bulky.Models;

namespace Bulky.DataAccess;

public interface IShoppingCartRepository : IRepository<ShoppingCart>
{
    void Update(ShoppingCart obj);

}
