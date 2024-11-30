using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace AHTB_TimBanCungGu_API.Models
{
    public class Phan
    {
        [Key]
        public string IDPhan { get; set; }

        public int SoPhan { get; set; }
        public DateTime NgayCongChieu { get; set; }
        [Required(ErrorMessage = "Vui lòng không được để trống Số lượng tập")]
        public int SoLuongTap { get; set; }
        [ForeignKey("Phim")]
        public string PhimID { get; set; }
        public Phim Phim { get; set; }
        public ICollection<Tap> Tap { get; set; }

    }
}
