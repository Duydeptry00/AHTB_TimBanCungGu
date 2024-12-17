using System.Collections.Generic;

namespace AHTB_TimBanCungGu_API.ViewModels
{
    public class MovieSession
    {
        public string SessionId { get; set; } // ID duy nhất của phiên xem phim
        public string MovieTitle { get; set; } // Tên bộ phim
        public string MovieUrl { get; set; } // URL của phim
        public List<string> Users { get; set; } // Danh sách người dùng tham gia phiên
        public int CurrentTime { get; set; } // Thời gian hiện tại của phim (giây)
        public bool IsPlaying { get; set; } // Trạng thái phát phim (true: đang phát, false: tạm dừng)
    }
}
