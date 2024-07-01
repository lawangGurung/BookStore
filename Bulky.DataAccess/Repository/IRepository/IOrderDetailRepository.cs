using Bulky.Models;

namespace Bulky.DataAccess;

public interface IOrderDetailRepository: IRepository<OrderDetail>
{
    void Update(OrderDetail obj);
}
