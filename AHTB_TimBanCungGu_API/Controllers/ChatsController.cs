using AHTB_TimBanCungGu_API.Chats;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Linq;
using MongoDB.Bson;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public ChatsController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(MessageVM messageVM)
        {
            try
            {
                // Chuyển đổi MessageVM sang Message
                var message = new Message
                {
                    Id = ObjectId.GenerateNewId(),
                    SenderUsername = messageVM.SenderUsername,
                    ReceiverUsername = messageVM.ReceiverUsername,
                    Content = messageVM.Content,
                    Timestamp = DateTime.UtcNow
                };

                // Kiểm tra xem cuộc trò chuyện giữa hai người đã tồn tại chưa
                var conversationFilter = Builders<Conversation>.Filter.And(
                    Builders<Conversation>.Filter.All(c => c.Participants, new List<string> { message.SenderUsername, message.ReceiverUsername }),
                    Builders<Conversation>.Filter.Size(c => c.Participants, 2) // Đảm bảo đúng 2 người tham gia
                );

                var conversation = await _context.Conversations.Find(conversationFilter).FirstOrDefaultAsync();

                // Nếu không tồn tại, tạo mới
                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        Participants = new List<string> { message.SenderUsername, message.ReceiverUsername },
                        LastMessageTimestamp = message.Timestamp
                    };
                    await _context.Conversations.InsertOneAsync(conversation);
                }
                else
                {
                    // Nếu đã tồn tại, cập nhật thời gian tin nhắn cuối
                    var update = Builders<Conversation>.Update.Set(c => c.LastMessageTimestamp, message.Timestamp);
                    await _context.Conversations.UpdateOneAsync(conversationFilter, update);
                }

                // Lưu tin nhắn
                await _context.Messages.InsertOneAsync(message);

                // Trả về thành công với dữ liệu tin nhắn
                return Ok(new
                {
                    Success = true,
                    Message = "Message sent successfully.",
                    Data = message
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while sending the message.",
                    Error = ex.Message
                });
            }
        }




        // GET api/Chats?receiverId=receiverId - Lấy danh sách tin nhắn của người nhận
        [HttpGet]
        public async Task<IActionResult> GetMessages([FromQuery] string ReceiverUserName, [FromQuery] string SenderUsername)
        {
            // Truy vấn danh sách tin nhắn giữa người gửi và người nhận từ MongoDB
            var filter = Builders<Message>.Filter.Or(
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(m => m.SenderUsername, SenderUsername),
                    Builders<Message>.Filter.Eq(m => m.ReceiverUsername, ReceiverUserName)
                ),
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(m => m.SenderUsername, ReceiverUserName),
                    Builders<Message>.Filter.Eq(m => m.ReceiverUsername, SenderUsername)
                )
            );

            var messages = await _context.Messages
                .Find(filter)
                .SortBy(m => m.Timestamp)  // Sắp xếp theo thời gian gửi
                .ToListAsync();

            // Chuyển đổi dữ liệu từ Message sang MessageVM
            var messageVMs = new List<MessageVM>();
            foreach (var message in messages)
            {
                messageVMs.Add(new MessageVM
                {
                    SenderUsername = message.SenderUsername,
                    ReceiverUsername = message.ReceiverUsername,
                    Content = message.Content,
                    Timestamp = message.Timestamp
                });
            }

            return Ok(messageVMs);  // Trả về danh sách MessageVM
        }
        [HttpPost("StartConversation")]
        public async Task<IActionResult> StartConversation(string SenderUsername, string ReceiverUsername)
        {
            // Tạo danh sách Participants và sắp xếp để đảm bảo thứ tự nhất quán
            var participants = new List<string> { SenderUsername, ReceiverUsername };
            participants.Sort();

            // Kiểm tra xem cuộc trò chuyện đã tồn tại chưa
            var filter = Builders<Conversation>.Filter.And(
                Builders<Conversation>.Filter.All(c => c.Participants, participants),
                Builders<Conversation>.Filter.Size(c => c.Participants, participants.Count)
            );

            var existingConversation = await _context.Conversations.Find(filter).FirstOrDefaultAsync();

            if (existingConversation == null)
            {
                // Nếu chưa tồn tại, tạo cuộc trò chuyện mới
                var conversation = new Conversation
                {
                    Participants = participants,
                    LastMessageTimestamp = null // Chưa có tin nhắn
                };

                await _context.Conversations.InsertOneAsync(conversation);
                return Ok(conversation); // Trả về thông tin cuộc trò chuyện mới tạo
            }

            // Nếu đã tồn tại, trả về thông tin cuộc trò chuyện đã tồn tại
            return Ok(existingConversation);
        }



        // GET api/Chats/Conversations?username=username - Lấy danh sách cuộc trò chuyện của người dùng
        [HttpGet("Conversations")]
        public async Task<IActionResult> GetConversations([FromQuery] string username)
        {
            // Truy vấn danh sách cuộc trò chuyện của người dùng
            var conversations = await _context.Conversations
                .Find(c => c.Participants.Contains(username))
                .SortByDescending(c => c.LastMessageTimestamp) // Sắp xếp theo thời gian gần nhất
                .ToListAsync();

            // Chuyển đổi các cuộc trò chuyện thành ConversationVM
            var conversationVMs = conversations.Select(c => new ConversationVM
            {
                id = c.Id.ToString(),  // Chuyển ObjectId thành string
                user1 = username, // Gán user1 là người gọi API (username)
                user2 = c.Participants.FirstOrDefault(p => p != username), // Gán user2 là người còn lại trong participants
                LastMessageTimestamp = c.LastMessageTimestamp
            }).ToList();

            return Ok(conversationVMs); // Trả về danh sách các cuộc trò chuyện dưới dạng ConversationVM
        }

    }
}
