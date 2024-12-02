using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AHTB_TimBanCungGu_API.ViewModels
{
    public class NhanVienVM
    {
        public int STT {  get; set; }
        public string IdNhanVien { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
        public string TrangThai { get; set; }
        public string Tenrole { get; set; }

    }
    public class NhanVienListViewModel
    {
        public IEnumerable<NhanVienVM> NhanViens { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public string UsernameFilter { get; set; }
        public string RoleFilter { get; set; }
        public int PageSize { get; set; }  // Số mục trên mỗi trang
    }

}
