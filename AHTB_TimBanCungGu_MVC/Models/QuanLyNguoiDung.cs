using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace AHTB_TimBanCungGu_MVC.Models
{
    public class QuanLyNguoiDung
    {
        [Key]
        public int IDQLND { get; set; }

        [ForeignKey("AdminUser")]
        public string AdminID { get; set; }

        [ForeignKey("NguoiDungUser")]
        public string NguoiDungID { get; set; }

        public string ThaoTac { get; set; }
        public DateTime MocThoiGian { get; set; }
        public DateTime LichSuMoKhoa { get; set; }
        public string LichSuLyDoKhoa { get; set; }

        [InverseProperty("QuanLyNguoiDungAdmin")]
        public virtual User AdminUser { get; set; }

        [InverseProperty("QuanLyNguoiDungNguoiDung")]
        public virtual User NguoiDungUser { get; set; }
    }
}
