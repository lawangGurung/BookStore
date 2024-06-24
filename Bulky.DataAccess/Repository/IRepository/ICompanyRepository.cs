using Bulky.Models;

namespace Bulky.DataAccess;

public interface ICompanyRepository: IRepository<Company>
{
    void Update (Company obj);
}
