using AHTB_TimBanCungGu_API.ViewModels;
using System.Collections.Generic;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    internal class AssignRolesViewModel
    {
        public List<NhanVienVM> Users { get; set; }
        public List<RoleVM> AvailableRoles { get; set; }
    }
}