using AHTB_TimBanCungGu_API.Models;
using System.Collections.Generic;

namespace AHTB_TimBanCungGu_API.ViewModels
{
    public class User_role
    {
       public string Username { get; set; }
        public int Id_Role { get; set; }
        public string TrangThai { get; set; }

    }
    public class ListUser_role {
        public List<User_role> RolesList { get; set; } = new List<User_role>(); // Danh sách các quyền truyền vào view
        public List<NhanVienVM> Users { get; set; } = new List<NhanVienVM>(); // Danh sách người dùng truyền vào view
        public List<RoleVM> Roles { get; set; } = new List<RoleVM>(); // Danh sách vai trò
        public int Role { get; set; } //Tên vai trò
        public List<string> User { get; set; } = new List<string>(); // Danh sách người dùng rỗng để xử lý
        public int Id_Role { get; set; } //Vai trò người dùng
        public List<ListPhanQuyen> PhanQuyen { get; set; } = new List<ListPhanQuyen>(); // Danh sách vai trò
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
    public class ListPhanQuyen
    {
        public int Id { get; set; }
        public string Module { get; set; }
        public string Add { get; set; }
        public string Update { get; set; }
        public string Delete { get; set; }
        public string ReviewDetails { get; set; }
        public string Username { set; get; }
        public string Tenrole { set; get; }
    }
}