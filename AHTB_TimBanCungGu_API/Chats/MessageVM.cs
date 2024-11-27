using System;

namespace AHTB_TimBanCungGu_API.Chats
{
    public class MessageVM
    {

        public string SenderUsername { get; set; }
        public string ReceiverUsername { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
