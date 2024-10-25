using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace AHTB_TimBanCungGu_MVC.Models
{
    public class CungGu
    {
        [Key]
        public int IdCungGu { get; set; }

        [ForeignKey("NguoiDungTimUser")]
        public string NguoiDungTim { get; set; }

        [ForeignKey("DoiTuongDuocTimUser")]
        public string DoiTuongDuocTim { get; set; }

        public DateTime ThoiGianTim { get; set; }
        public string HuongVuot { get; set; }

        [InverseProperty("CungGuNguoiDungTim")]
        public virtual User NguoiDungTimUser { get; set; }

        [InverseProperty("CungGuDoiTuongDuocTim")]
        public virtual User DoiTuongDuocTimUser { get; set; }
    }
}
