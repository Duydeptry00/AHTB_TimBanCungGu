using System;
using System.Collections.Generic;

namespace AHTB_TimBanCungGu_API.ViewModels
{
    public class Profile
    {
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string SoDienThoai { get; set; }
        public bool IsPremium { get; set; }
        public string MoTa { get; set; }
        public string DiaChi { get; set; }
        public List<string> Avt { get; set;}
    }
}
