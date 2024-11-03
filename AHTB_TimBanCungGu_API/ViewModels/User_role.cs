using AHTB_TimBanCungGu_API.Models;
using System.Collections.Generic;

namespace AHTB_TimBanCungGu_API.ViewModels
{
    public class User_role
    {
       public string Id_User { get; set; }
        public int Id_Role { get; set; }
        public string Tenrole { get; set; }
    }
    public class ListUser_role {
        public List<User_role> RolesList { get; set; } = new List<User_role>(); // Danh sách các quyền
        public List<NhanVienVM> Users { get; set; } = new List<NhanVienVM>(); // Danh sách người dùng
        public List<RoleVM> Roles { get; set; } = new List<RoleVM>(); // Danh sách vai trò
    }
}