using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AHTB_TimBanCungGu_API.Chats
{
    public class MatchNguoiDung
    {
        [BsonId]
        public ObjectId Id { get; set; } // Typically, an ObjectId for MongoDB documents.

        [BsonElement("IDSwipe")]
        public string IDSwipe { get; set; } // Ensure this matches the MongoDB field name.

        [BsonElement("User1")]
        public string User1 { get; set; }

        [BsonElement("User2")]
        public string User2 { get; set; }

        [BsonElement("SwipeAction")]
        public string SwipeAction { get; set; } // Can be "Like" or "Dislike".

        [BsonElement("SwipedAt")]
        public DateTime SwipedAt { get; set; }
    }
}
