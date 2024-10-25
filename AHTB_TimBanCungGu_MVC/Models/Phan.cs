using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AHTB_TimBanCungGu_MVC.Models
{
    public class Phan
    {
        [Key]
        public string IDPhan { get; set; }
        public int SoPhan { get; set; }
        public string NgayCongChieu { get; set; }
        public int SoLuongTap { get; set; }
        [ForeignKey("Phim")]
        public string PhimID { get; set; }
        public Phim Phim { get; set; }
        public ICollection<Tap> Tap { get; set; }

    }
}
