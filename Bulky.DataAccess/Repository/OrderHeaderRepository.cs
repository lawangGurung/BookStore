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

    public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
    {
        OrderHeader? orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
        if (orderHeaderFromDb is not null)
        {
            orderHeaderFromDb.OrderStatus = orderStatus;
            if(!string.IsNullOrEmpty(paymentStatus))
            {
                orderHeaderFromDb.PaymentStatus = paymentStatus;
            }
        }
    }

    public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
    {
        OrderHeader? orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
        if(orderHeaderFromDb is not null)
        {
            if(!string.IsNullOrEmpty(sessionId))
            {
                orderHeaderFromDb.SessionId = sessionId;
            }

            if(!string.IsNullOrEmpty(paymentIntentId))
            {
                orderHeaderFromDb.PaymentIntentId = paymentIntentId;
                orderHeaderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
