using Bulky.DataAccess.Data;
using Bulky.Models;

namespace Bulky.DataAccess;

public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
{
    private readonly ApplicationDbContext _db;
    public OrderHeaderRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }
    public void Update(OrderHeader obj)
    {
        _db.OrderHeaders.Update(obj);
    }
}
