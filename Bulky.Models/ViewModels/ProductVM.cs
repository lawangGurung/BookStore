using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bulky.Models;

public class ProductVM
{
    public required Product Product { get; set; }
    [ValidateNever]
    public required IEnumerable<SelectListItem> CategoryList { get; set; }
}
