﻿using Bulky.DataAccess.Data;
using Bulky.Models;

namespace Bulky.DataAccess;

public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
{
    private readonly ApplicationDbContext _db;
    public ShoppingCartRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }


    public void Update(ShoppingCart obj)
    {
        _db.ShoppingCarts.Update(obj);
    }
}
