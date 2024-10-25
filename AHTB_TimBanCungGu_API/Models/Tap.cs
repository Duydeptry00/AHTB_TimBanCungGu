using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AHTB_TimBanCungGu_API.Models
{
    public class Tap
    {
        [Key]
        public string IDTap { get; set; }
        public int SoTap { get; set; }
        [ForeignKey("Phan")]
        public string PhanPhim { get; set; }
        public Phan Phan { get; set; }
    }
}
