using Bulky.Models;

namespace Bulky.DataAccess;

public interface IProductRepository : IRepository<Product> 
{
   void Update(Product obj); 
}
