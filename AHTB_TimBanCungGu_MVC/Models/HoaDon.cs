using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHTB_TimBanCungGu_MVC.Models
{
    public class HoaDon
    {
        [Key]
        public int IDHoaDon { get; set; }
        [ForeignKey("User")]
        public string NguoiMua { get; set; }
        public User User { get; set; }
        public string GoiPremium { get; set; }
        public DateTime NgayHetHan { get; set; }
        public double TongTien { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string TrangThai { get; set; }
        public DateTime NgayThanhToan { get; set; }
    }
}
