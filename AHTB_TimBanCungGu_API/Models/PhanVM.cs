using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AHTB_TimBanCungGu_API.Models
{
    public class PhanVM
    {
        public string IDPhan { get; set; }
        public int SoPhan { get; set; }
        public DateTime NgayCongChieu { get; set; }
        public int SoLuongTap { get; set; }
        
        public string PhimID { get; set; }
      
    }
}
