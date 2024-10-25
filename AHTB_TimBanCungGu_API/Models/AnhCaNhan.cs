using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHTB_TimBanCungGu_API.Models
{
    public class AnhCaNhan
    {
        [Key]
        public int IDAnhCN { get; set; }

        public string HinhAnh { get; set; }

        [ForeignKey("ThongTinCaNhan")]
        public int IDProfile { get; set; }

        public ThongTinCaNhan ThongTinCN { get; set; }
    }
}
