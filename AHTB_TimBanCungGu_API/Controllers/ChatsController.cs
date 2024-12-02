using AHTB_TimBanCungGu_API.Chats;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Linq;
using MongoDB.Bson;
using AHTB_TimBanCungGu_API.Data;
using Microsoft.EntityFrameworkCore.Query;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly DBAHTBContext _DBcontext;
        private readonly MongoDbContext _context;

        public ChatsController(MongoDbContext context, DBAHTBContext DBcontext)
        {
            _context = context;
            _DBcontext = DBcontext;
        }
        [HttpGet("Profiles")]
        public async Task<ActionResult> GetProfiles(string username)
        {
            var profile = await _DBcontext.ThongTinCN
              .Where(t => t.User.UserName == username) // Lọc theo username
              .Select(t => new Profile
              {
                  HoTen = string.IsNullOrEmpty(t.HoTen) ? "Không có thông tin" : t.HoTen,
                  GioiTinh = string.IsNullOrEmpty(t.GioiTinh) ? "Không có thông tin" : t.GioiTinh,
                  NgaySinh = t.NgaySinh,
                  SoDienThoai = string.IsNullOrEmpty(t.SoDienThoai) ? "Không có thông tin" : t.SoDienThoai,
                  IsPremium = t.IsPremium,
                  MoTa = string.IsNullOrEmpty(t.MoTa) ? "Không có thông tin" : t.MoTa,
                  DiaChi = string.IsNullOrEmpty(t.DiaChi) ? "Không có thông tin" : t.DiaChi,
                  Avt = t.AnhCaNhan.Select(a => a.HinhAnh).ToList()
              })
              .FirstOrDefaultAsync();


            if (profile == null)
            {
                return NotFound("Không tìm thấy dữ liệu.");
            }

            // Nếu danh sách ảnh rỗng hoặc null, gán giá trị mặc định
            if (profile.Avt == null || !profile.Avt.Any())
            {
                profile.Avt = new List<string> { "AnhCN.jpg" };
            }

            return Ok(profile);
        }
        [HttpPost("BaoCao")]
        public async Task<ActionResult<BaoCao>> CreateBaoCao(BaoCao baoCaoNguoiDung)
        {
            if (baoCaoNguoiDung == null)
            {
                return Ok(new { Success = false, Message = "Dữ liệu báo cáo không hợp lệ." });
            }
            var NguoiBaoCao = _DBcontext.Users.FirstOrDefault(u => u.UserName == baoCaoNguoiDung.NguoiBaoCao);
            var DoiTuongBaoCao = _DBcontext.Users.FirstOrDefault(u => u.UserName == baoCaoNguoiDung.DoiTuongBaoCao);
            // Kiểm tra xem đã có báo cáo nào từ người báo cáo đến đối tượng báo cáo trong ngày hôm nay chưa
            var existingReport = await _DBcontext.BaoCaoNguoiDung
                .Where(b => b.NguoiBaoCao == NguoiBaoCao.UsID &&
                            b.DoiTuongBaoCao == DoiTuongBaoCao.UsID &&
                            b.NgayBaoCao.Date == DateTime.Today)
                .FirstOrDefaultAsync();

            if (existingReport != null)
            {
                return Ok(new { Success = false, Message = "Bạn đã báo cáo người dùng này trong ngày hôm nay. Vui lòng chờ xét duyệt." });
            }



            if (NguoiBaoCao == null || DoiTuongBaoCao == null)
            {
                return Ok(new { Success = false, Message = "Không tìm thấy người dùng để báo cáo." });
            }

            // Tạo mới đối tượng BaoCao từ BaoCaoNguoiDung
            var baoCao = new BaoCaoNguoiDung
            {
                NguoiBaoCao = NguoiBaoCao.UsID,
                DoiTuongBaoCao = DoiTuongBaoCao.UsID,
                LyDoBaoCao = baoCaoNguoiDung.LyDoBaoCao,
                NgayBaoCao = DateTime.Now, // Đặt ngày báo cáo là thời gian hiện tại
                TrangThai = "Đang Chờ Duyệt" // Trạng thái mặc định
            };

            // Thêm báo cáo vào cơ sở dữ liệu SQL
            _DBcontext.BaoCaoNguoiDung.Add(baoCao);
            await _DBcontext.SaveChangesAsync();

            // Trích xuất các tin nhắn liên quan từ MongoDB
            var messages = await _context.Messages
                .Find(m => m.SenderUsername == baoCaoNguoiDung.NguoiBaoCao && m.ReceiverUsername == baoCaoNguoiDung.DoiTuongBaoCao)
                .ToListAsync();

            // Tạo đối tượng `NoiDungBaoCao` và lưu vào MongoDB
            var noiDungBaoCao = new NoiDungBaoCao
            {
                NguoiBaoCao = baoCaoNguoiDung.NguoiBaoCao,
                DoiTuongBaoCao = baoCaoNguoiDung.DoiTuongBaoCao,
                NgayBaoCao = DateTime.Now,
                LyDoBaoCao = baoCaoNguoiDung.LyDoBaoCao,
                TinNhanLienQuan = messages.Select(m => m.Content).ToList()
            };

            await _context.NoiDungBaoCao.InsertOneAsync(noiDungBaoCao);

            return Ok(new { Success = true, Message = "Báo cáo đã được gửi thành công." });
        }



        [HttpGet("CheckBlockStatus")]
        public async Task<IActionResult> CheckBlockStatus([FromQuery] string ReceiverUserName, [FromQuery] string SenderUsername)
        {
            try
            {
                // Kiểm tra trạng thái chặn từ Sender -> Receiver
                var senderBlockedReceiver = await _context.BlockUser
                    .Find(b => b.BlockerUsername == SenderUsername && b.BlockedUsername == ReceiverUserName)
                    .FirstOrDefaultAsync();

                // Kiểm tra trạng thái chặn từ Receiver -> Sender
                var receiverBlockedSender = await _context.BlockUser
                    .Find(b => b.BlockerUsername == ReceiverUserName && b.BlockedUsername == SenderUsername)
                    .FirstOrDefaultAsync();

                // Lấy thông tin người dùng
                var receiverInfo = _DBcontext.ThongTinCN.FirstOrDefault(t => t.User.UserName == ReceiverUserName);

                if (senderBlockedReceiver != null)
                {
                    return Ok(new
                    {
                        Success = true,
                        DaChan = true,
                        BlockDirection = SenderUsername, // Sender đã chặn Receiver
                        Message = $"Bạn đã chặn {receiverInfo?.HoTen ?? "người dùng này"}."
                    });
                }

                if (receiverBlockedSender != null)
                {
                    return Ok(new
                    {
                        Success = true,
                        DaChan = true,
                        BlockDirection = ReceiverUserName, // Receiver đã chặn Sender
                        Message = $"{receiverInfo?.HoTen ?? "Người dùng này"} đã chặn bạn."
                    });
                }

                // Nếu không có ai chặn ai
                return Ok(new
                {
                    Success = true,
                    DaChan = false,
                    Message = "Không có trạng thái chặn giữa hai người dùng."
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi khi kiểm tra trạng thái chặn.",
                    Loi = ex.Message
                });
            }
        }

        [HttpPost("UnblockUser")]
        public async Task<IActionResult> UnblockUser(BlockUserRequest user)
        {
            try
            {
                string SenderUsername = user.SenderUsername;
                string ReceiverUserName = user.ReceiverUsername;
                // Tìm bản ghi chặn trong MongoDB
                var blockEntry = await _context.BlockUser
                    .Find(b => b.BlockerUsername == SenderUsername && b.BlockedUsername == ReceiverUserName)
                    .FirstOrDefaultAsync();

                if (blockEntry == null)
                {
                    // Nếu không tìm thấy bản ghi, trả về thông báo
                    return BadRequest(new { Success = false, Message = "Người dùng này chưa bị chặn." });
                }

                // Xóa bản ghi chặn
                var deleteResult = await _context.BlockUser
                    .DeleteOneAsync(b => b.Id == blockEntry.Id);

                if (deleteResult.DeletedCount > 0)
                {
                    // Nếu xóa thành công, trả về thông báo
                    return Ok(new { Success = true, Message = "Đã bỏ chặn người dùng thành công." });
                }

                // Nếu không xóa được, trả về lỗi
                return StatusCode(500, new { Success = false, Message = "Đã xảy ra lỗi khi bỏ chặn người dùng." });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                return StatusCode(500, new { Success = false, Message = "Đã xảy ra lỗi khi bỏ chặn người dùng.", Loi = ex.Message });
            }
        }


        [HttpPost("BlockUser")]
        public async Task<IActionResult> BlockUser(BlockUserRequest user)
        {
            try
            {
                string SenderUsername = user.SenderUsername;
                string ReceiverUserName = user.ReceiverUsername;
                // Kiểm tra nếu người dùng tự chặn chính mình
                if (SenderUsername == ReceiverUserName)
                {
                    // Phản hồi lỗi nếu người dùng tự chặn chính mình
                    return Ok(new { Success = false, Message = "Bạn không thể tự chặn chính mình." });
                }

                // Kiểm tra xem việc chặn này đã tồn tại chưa
                var existingBlock = await _context.BlockUser
                    .Find(b => b.BlockerUsername == SenderUsername && b.BlockedUsername == ReceiverUserName)
                    .FirstOrDefaultAsync();

                if (existingBlock != null)
                {
                    // Phản hồi lỗi nếu người dùng đã bị chặn trước đó
                    return Ok(new { Success = false, Message = "Người dùng này đã bị chặn trước đó." });
                }

                // Tạo một bản ghi chặn mới
                var blockUser = new BlockUser
                {
                    Id = ObjectId.GenerateNewId(), // Tự động tạo ID mới
                    BlockerUsername = SenderUsername, // Người thực hiện chặn
                    BlockedUsername = ReceiverUserName, // Người bị chặn
                    BlockDate = DateTime.UtcNow, // Thời gian thực hiện chặn
                };

                // Thêm bản ghi vào MongoDB
                await _context.BlockUser.InsertOneAsync(blockUser);

                // Phản hồi thành công
                return Ok(new { Success = true, Message = "Đã chặn người dùng thành công." });
            }
            catch (Exception ex)
            {
                // Xử lý và phản hồi lỗi hệ thống
                return StatusCode(500, new { Success = false, Message = "Đã xảy ra lỗi khi thực hiện chặn người dùng.", Loi = ex.Message });
            }
        }
        public class BlockUserRequest
        {
            public string ReceiverUsername { get; set; }
            public string SenderUsername { get; set; }
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


        [HttpGet("CheckMatchSwipeAction")]
        public async Task<IActionResult> CheckMatchSwipeAction([FromQuery] string username)
        {
            try
            {
                // Find matches where the given username exists in either User1 or User2
                var filter = Builders<MatchNguoiDung>.Filter.Or(
                    Builders<MatchNguoiDung>.Filter.Eq(m => m.User1, username)
                );
                var filter2 = Builders<MatchNguoiDung>.Filter.Or(
                    Builders<MatchNguoiDung>.Filter.Eq(m => m.User2, username)
                );
                // Find all matching records for the given username
                var matches = await _context.MatchNguoiDung
                    .Find(filter)
                    .ToListAsync();
                // Find all matching records for the given username
                var matches2 = await _context.MatchNguoiDung
                    .Find(filter2)
                    .ToListAsync();
                // Check if there are any matches
                if (matches != null && matches.Count > 0 && matches2 != null)
                {
                    // Filter matches where SwipeAction is "Like"
                    var likedMatches = matches.Where(m => m.SwipeAction == "Like").ToList();

                    // Filter matches where SwipeAction is "Like"
                    var likedMatches2 = matches2.Where(m => m.SwipeAction == "Like").ToList();
                    // To store users who have a mutual match
                    var mutualMatches = new List<string>();

                    foreach (var match in likedMatches)
                    {
                       foreach (var match2 in likedMatches2)
                       {
                            if (match.User1 == match2.User2 && match.User2 == match2.User1)
                            {
                                mutualMatches.Add(match.User2);
                            }
                       }
                    }

                    // Ensure unique results
                    mutualMatches = mutualMatches.Distinct().ToList();

                    // If there are mutual matches, return them; otherwise, return a no-match message
                    if (mutualMatches.Count > 0)
                    {
                        return Ok(new
                        {
                            Success = true,
                            Message = "Both users have a mutual 'Like'.",
                            MatchedUsers = mutualMatches
                        });
                    }

                    return Ok(new { Success = false, Message = "No mutual 'Like' found." });
                }

                // If no matches are found
                return Ok(new { Success = false, Message = "No matching records found." });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return StatusCode(500, new { Success = false, Message = "An error occurred while checking the match.", Error = ex.Message });
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
            var Name1 = _DBcontext.ThongTinCN.FirstOrDefault(N1 => N1.User.UserName == SenderUsername);
            var Name2 = _DBcontext.ThongTinCN.FirstOrDefault(N1 => N1.User.UserName == ReceiverUserName);
            var messages = await _context.Messages
                .Find(filter)
                .SortBy(m => m.Timestamp)  // Sắp xếp theo thời gian gửi
                .ToListAsync();
            // Lấy ảnh đầu tiên từ danh sách ảnh của người dùng dưới dạng chuỗi
            var Avt2 = _DBcontext.AnhCaNhan
                .Where(N1 => N1.ThongTinCN.User.UserName == SenderUsername)
                .Select(N1 => N1.HinhAnh.FirstOrDefault().ToString()) // Chuyển ký tự đầu tiên thành chuỗi
                .FirstOrDefault();
            // Chuyển đổi dữ liệu từ Message sang MessageVM
            var messageVMs = new List<MessageVM>();
            foreach (var message in messages)
            {
                messageVMs.Add(new MessageVM
                {
                    SenderUsername = message.SenderUsername,
                    SenderName = Name1.HoTen,
                    ReceiverUsername = message.ReceiverUsername,
                    ReceiverName = Name2.HoTen,
                    Avt2 = Avt2 ?? "anhCN.jpg",
                    Content = message.Content,
                    Timestamp = message.Timestamp
                });
            }

            return Ok(messageVMs);  // Trả về danh sách MessageVM
        }

        [HttpPost("StartConversation")]
        public async Task<IActionResult> StartConversation([FromBody] StartConversationRequest request)
        {
            string SenderUsername = request.SenderUsername;
            string ReceiverUsername = request.ReceiverUsername;
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

            return Ok(existingConversation);
        }

        public class StartConversationRequest
        {
            public string SenderUsername { get; set; }
            public string ReceiverUsername { get; set; }
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
            // Truy xuất tất cả các Participants từ danh sách conversations và lọc theo username
            var participantsList = conversations
                .SelectMany(c => c.Participants) // Truy xuất tất cả các Participants từ mỗi cuộc trò chuyện
                .Where(p => p != username)       // Loại bỏ username của người dùng chính
                .Distinct()                      // Loại bỏ các phần tử trùng lặp
                .ToList();                       // Chuyển đổi sang List<string>
            var Name1 = _DBcontext.ThongTinCN.FirstOrDefault(N1 => N1.User.UserName == username);


            // Danh sách ConversationVM để trả về
            var conversationVMs = new List<ConversationVM>();

            foreach (var conversation in conversations)
            {

                string name2 = null;
                string avt2 = null;
                // Lấy tài khoản khác UserName của conversation.user1
                var otherUserName = conversation.Participants
                    .FirstOrDefault(participant => participant != username);
                var Name2 = _DBcontext.ThongTinCN.FirstOrDefault(N1 => N1.User.UserName == otherUserName);
                if(Name2 != null)
                {
                    name2 = Name2.HoTen;
                }
                // Lấy ảnh đầu tiên từ danh sách ảnh của người dùng dưới dạng chuỗi
                var Avt2 = _DBcontext.AnhCaNhan
                    .Where(N1 => N1.ThongTinCN.User.UserName == otherUserName)
                    .Select(N1 => N1.HinhAnh)
                    .FirstOrDefault();
                if (Avt2 != null)
                {
                    avt2 = avt2;
                }
                // Lấy tin nhắn mới nhất cho cuộc trò chuyện này
                var latestMessage = await _context.Messages
                    .Find(m => conversation.Participants.Contains(m.SenderUsername) &&
                               conversation.Participants.Contains(m.ReceiverUsername))
                    .SortByDescending(m => m.Timestamp)
                    .FirstOrDefaultAsync();

                // Chuyển đổi sang ConversationVM
                conversationVMs.Add(new ConversationVM
                {
                    id = conversation.Id.ToString(),
                    user1 = username, // Gán user1 là người gọi API
                    hotenuser1 = Name1.HoTen,
                    user2 = conversation.Participants.FirstOrDefault(p => p != username), // Gán user2 là người còn lại trong participants
                    hotenuser2 = name2 ?? "Chưa có họ tên",
                    Avatar = Avt2 ?? "anhCN.jpg",
                    contentnew = latestMessage != null ? $"{latestMessage.SenderUsername}: {latestMessage.Content}" : null, // Nội dung tin nhắn mới nhất với người gửi
                    LastMessageTimestamp = conversation.LastMessageTimestamp
                });
            }

            return Ok(conversationVMs); // Trả về danh sách các cuộc trò chuyện dưới dạng ConversationVM
        }


    }
}
