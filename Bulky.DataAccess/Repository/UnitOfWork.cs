﻿using Bulky.DataAccess.Data;
using Bulky.Models;

namespace Bulky.DataAccess;

public class UnitOfWork : IUnitOfWork
{
    public ICategoryRepository Category {get; private set;}
    public IProductRepository Product { get; private set; }
    public ICompanyRepository Company { get; set; }
    public IShoppingCartRepository ShoppingCart { get; set; }
    private ApplicationDbContext _db;

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        Category = new CategoryRepository(_db);
        Product = new ProductRepository(_db);
        Company = new CompanyRepository(_db);
        ShoppingCart = new ShoppingCartRepository(_db);
    }
    public void Save()
    {
        _db.SaveChanges();
    }
}
