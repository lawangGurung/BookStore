using Bulky.Models;

public class ShoppingCartVM
{
    public IEnumerable<ShoppingCart>? ShoppingCartList { get; set; }
    public required OrderHeader OrderHeader { get; set; }
}