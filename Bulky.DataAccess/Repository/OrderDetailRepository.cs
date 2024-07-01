using Bulky.DataAccess.Data;
using Bulky.Models;

namespace Bulky.DataAccess;

public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
{
    private readonly ApplicationDbContext _db;

    public OrderDetailRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
    public void Update(OrderDetail obj)
    {
        _db.OrderDetails.Update(obj);
    }
}
