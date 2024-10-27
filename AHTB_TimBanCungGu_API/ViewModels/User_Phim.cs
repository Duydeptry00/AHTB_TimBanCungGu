using AHTB_TimBanCungGu_API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace AHTB_TimBanCungGu_API.ViewModels
{
    public class User_Phim
    {
        public string IDPhim { get; set; }
        public string TenPhim { get; set; }
        public string MoTa { get; set; }
        public string DienVien { get; set; }
        public string TheLoaiPhim { get; set; }
        public TheLoai TheLoai { get; set; }
        public DateTime NgayPhatHanh { get; set; }
        public double DanhGia { get; set; }
        public string TrailerURL { get; set; }
        public bool NoiDungPremium { get; set; }
        public string SourcePhim { get; set; }
        public string HinhAnh { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public string username { get; set; }
    }
}
