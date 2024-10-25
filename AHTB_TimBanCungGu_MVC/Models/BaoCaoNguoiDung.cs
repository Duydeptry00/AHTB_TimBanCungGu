using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace AHTB_TimBanCungGu_MVC.Models
{
    public class BaoCaoNguoiDung
    {
        [Key]
        public int IDReport { get; set; }

        [ForeignKey("NguoiBaoCaoUser")]
        public string NguoiBaoCao { get; set; }

        [ForeignKey("DoiTuongBaoCaoUser")]
        public string DoiTuongBaoCao { get; set; }

        public string LyDoBaoCao { get; set; }
        public DateTime NgayBaoCao { get; set; }
        public string TrangThai { get; set; }

        [InverseProperty("ReportsNguoiBaoCao")]
        public virtual User NguoiBaoCaoUser { get; set; }

        [InverseProperty("ReportsDoiTuongBaoCao")]
        public virtual User DoiTuongBaoCaoUser { get; set; }
    }
}
