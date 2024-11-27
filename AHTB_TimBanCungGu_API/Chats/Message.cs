using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace AHTB_TimBanCungGu_API.Chats
{
    public class Message
    {
        [BsonId]  // Gán cho trường _id của MongoDB
        public ObjectId Id { get; set; }  // Kiểu ObjectId sẽ được MongoDB tự động tạo

        public string SenderUsername { get; set; }
        public string ReceiverUsername { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
