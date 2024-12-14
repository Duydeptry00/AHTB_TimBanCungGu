using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHTB_TimBanCungGu_API.Models
{
    public class ThongTinCaNhan
    {
        [Key]
        public int IDProfile { get; set; }

        [ForeignKey("User")]
        public string UsID { get; set; }

        public User User { get; set; }

        public string Email { get; set; }
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string SoDienThoai { get; set; }
        public bool IsPremium { get; set; }
        public string MoTa { get; set; }
        public string DiaChi { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; }
        public string ThichNhat { get; set; }
        public string Facebook { get; set; }
        public string Instagram { get; set; }

        public ICollection<AnhCaNhan> AnhCaNhan { get; set; }
    }
}
