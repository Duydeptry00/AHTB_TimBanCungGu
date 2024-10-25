using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AHTB_TimBanCungGu_API.Models
{
    public class PhimYeuThich
    {
        [Key]
        public int IdYeuThich { get; set; }
        [ForeignKey("User")]
        public string NguoiDungYT { get; set; }
        public User User { get; set; }
        [ForeignKey("Phim")]
        public string PhimYT { get; set; }
        public Phim Phim { get; set; }
    }
}
