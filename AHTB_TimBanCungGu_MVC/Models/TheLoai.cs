using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AHTB_TimBanCungGu_MVC.Models
{
    public class TheLoai
    {
        [Key]
        public string IdTheLoai { get; set; }
        public string TenTheLoai { get; set; }
        public ICollection<Phim> Phim { get; set; }
    }
}
