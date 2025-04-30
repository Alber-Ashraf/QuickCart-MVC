using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickCart.Data;
using QuickCart.DataAccess.Repository.IRepository;
using QuickCart.Models;

namespace QuickCart.DataAccess.Repository
{
    class CompanyRepository : Repository<Company> , ICompanyRepository 
    {   
        private readonly QuickCartDbContext _db;
        public CompanyRepository(QuickCartDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Company obj)
        {
            // Find the company in the database using its Id
            var objFromDb = _db.Companies.Find(obj.Id);

            // If the company is found, update its properties
            if (obj != null)
            {
                objFromDb.Name = obj.Name;
                objFromDb.Address = obj.Address;
                objFromDb.City = obj.City;
                objFromDb.State = obj.State;
                objFromDb.PostalCode = obj.PostalCode;
                objFromDb.Phone = obj.Phone;
            }
        }
    }
}
