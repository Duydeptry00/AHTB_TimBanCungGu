using System.Collections.Generic;
using System;

namespace AHTB_TimBanCungGu_API.ViewModels
{
    public class InfoNguoiDung
    {
        public int IDProfile { get; set; }
        public string UsID { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string GioiTinh { get; set; }
        public DateTime NgaySinh { get; set; }
        public string SoDienThoai { get; set; }
        public bool IsPremium { get; set; }
        public string MoTa { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; }

        // Các thông tin về ảnh cá nhân
        public List<string> HinhAnh { get; set; }
    }
}
