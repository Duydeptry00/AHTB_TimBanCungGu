using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AHTB_TimBanCungGu_API.Chats
{
    public class Conversation
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public List<string> Participants { get; set; } = new List<string>(); // Danh sách người tham gia

        public DateTime? LastMessageTimestamp { get; set; } // Null nếu chưa có tin nhắn
    }
}