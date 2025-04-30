using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCart.Models;

namespace QuickCart.DataAccess.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        // Define methods specific to Company repository
        void Update(Company obj);
    }
}
