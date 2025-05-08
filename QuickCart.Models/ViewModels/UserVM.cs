using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuickCart.Models.ViewModels
{
    public class UserVM
    {
        public ApplicationUser ApplicationUser { get; set; }
        public IEnumerable<SelectListItem> Company_List { get; set; }
        public IEnumerable<SelectListItem> Role_List { get; set; }
    }
}
