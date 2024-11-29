using System;

namespace AHTB_TimBanCungGu_MVC.Models
{
    public class Message
    {
        public MongoDB.Bson.ObjectId Id { get; set; }
        public string SenderUserName { get; set; }
        public string ReceiverUserName { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        // Constructor to initialize the message with necessary details
        public Message(string senderUserName, string receiverUserName, string content)
        {
            SenderUserName = senderUserName;
            ReceiverUserName = receiverUserName;
            Content = content;
            Timestamp = DateTime.Now;  // Automatically set the timestamp when message is created
        }
    }
}
