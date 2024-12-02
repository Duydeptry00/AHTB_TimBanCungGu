using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace AHTB_TimBanCungGu_API.Chats
{
    public class BlockUser
    {
        [BsonId] // Định danh chính trong MongoDB (_id)
        public ObjectId Id { get; set; } // Tự động tạo bởi MongoDB

        [BsonElement("blockerUsername")]
        public string BlockerUsername { get; set; } // Người chặn

        [BsonElement("blockedUsername")]
        public string BlockedUsername { get; set; } // Người bị chặn

        [BsonElement("blockDate")]
        public DateTime BlockDate { get; set; } // Thời gian thực hiện chặn
    }
}
