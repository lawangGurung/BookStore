namespace Bulky.DataAccess;

public interface IUnitOfWork
{
    ICategoryRepository Category {get;}
    IProductRepository Product {get;}
    ICompanyRepository Company { get;}
    IShoppingCartRepository ShoppingCart { get; set; }
    void Save();
}
