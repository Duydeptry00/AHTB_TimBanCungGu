using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace AHTB_TimBanCungGu_MVC.Models
{
    public class LichSuXem
    {
        [Key]
        public int IDLSX { get; set; }
        [ForeignKey("User")]
        public string NguoiDungXem { get; set; }
        public User User { get; set; }
        [ForeignKey("Phim")]
        public string PhimDaXem { get; set; }
        public Phim Phim { get; set; }
        public DateTime ThoiGianXem { get; set; }
    }
}
