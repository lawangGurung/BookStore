using Bulky.Models;

namespace Bulky.DataAccess;

public interface IOrderHeaderRepository : IRepository<OrderHeader>
{
    void Update(OrderHeader obj);
        
}
