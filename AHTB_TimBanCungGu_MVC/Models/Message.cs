using System;

namespace AHTB_TimBanCungGu_MVC.Models
{
    // Định nghĩa lớp Message
    public class Message
    {
        public string SenderUserName { get; set; }
        public string ReceiverUserName { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
