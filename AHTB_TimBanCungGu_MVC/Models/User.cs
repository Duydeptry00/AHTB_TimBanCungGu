using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHTB_TimBanCungGu_MVC.Models
{
    public class User
    {
        [Key]
        public string UsID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string TrangThai { get; set; }
        public ThongTinCaNhan ThongTinCN { get; set; }
        public ICollection<User_Role> User_Role { get; set; }
        public ICollection<HoaDon> HoaDon { get; set; }
        public ICollection<Phim> Phim { get; set; }
        [InverseProperty("NguoiBaoCaoUser")]
        public virtual ICollection<BaoCaoNguoiDung> ReportsNguoiBaoCao { get; set; }

        [InverseProperty("DoiTuongBaoCaoUser")]
        public virtual ICollection<BaoCaoNguoiDung> ReportsDoiTuongBaoCao { get; set; }
        [InverseProperty("AdminUser")]
        public virtual ICollection<QuanLyNguoiDung> QuanLyNguoiDungAdmin { get; set; }

        [InverseProperty("NguoiDungUser")]
        public virtual ICollection<QuanLyNguoiDung> QuanLyNguoiDungNguoiDung { get; set; }
        public ICollection<PhimYeuThich> PhimYeuThich { get; set; }
        public ICollection<LichSuXem> LichSuXem { get; set; }
        [InverseProperty("NguoiDungTimUser")]
        public virtual ICollection<CungGu> CungGuNguoiDungTim { get; set; }

        [InverseProperty("DoiTuongDuocTimUser")]
        public virtual ICollection<CungGu> CungGuDoiTuongDuocTim { get; set; }
    }
}
