using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace AHTB_TimBanCungGu_API.Chats
{
    public class BaoCao
    {
        public string NguoiBaoCao { get; set; }
        public string DoiTuongBaoCao { get; set; }
        public string LyDoBaoCao { get; set; }
    }
}
