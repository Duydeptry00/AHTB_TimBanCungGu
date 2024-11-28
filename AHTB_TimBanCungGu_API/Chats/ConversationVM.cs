using System.Collections.Generic;
using System;

namespace AHTB_TimBanCungGu_API.Chats
{
    public class ConversationVM
    {
        public string id { get; set; }
        public string user1 { get; set; }
        public string hotenuser1 { get; set; }
        public string user2 { get; set; }
        public string hotenuser2 { get; set; }
        public string contentnew { get; set; }

        public DateTime? LastMessageTimestamp { get; set; } // Null nếu chưa có tin nhắn
    }
}

