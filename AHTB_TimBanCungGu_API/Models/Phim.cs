using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace AHTB_TimBanCungGu_API.Models
{

    public class Phim
    {
        [Key]
        public string IDPhim { get; set; }
        public string TenPhim { get; set; }
        public string MoTa { get; set; }
        public string DienVien { get; set; }
        [ForeignKey("TheLoai")]
        public string TheLoaiPhim { get; set; }
        public TheLoai TheLoai { get; set; }
        public DateTime NgayPhatHanh { get; set; }
        public double DanhGia { get; set; }
        public string TrailerURL { get; set; }
        public bool NoiDungPremium { get; set; }
        public string SourcePhim { get; set; }
        public string HinhAnh { get; set; }
        public string DangPhim { get; set; }
        public DateTime NgayCapNhat { get; set; }
        [ForeignKey("User")]
        public string IDAdmin { get; set; }
        public User User { get; set; }
        public string TrangThai { get; set; }
        public ICollection<Phan> Phan { get; set; }
        public ICollection<PhimYeuThich> PhimYeuThich { get; set; }
        public ICollection<LichSuXem> LichSuXem { get; set; }
    }
}
