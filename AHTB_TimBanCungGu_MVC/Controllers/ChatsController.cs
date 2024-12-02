using Microsoft.AspNetCore.Mvc;
using AHTB_TimBanCungGu_MVC.Models;
using AHTB_TimBanCungGu_API.Chats;  // Import model ConversationVM
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using AHTB_TimBanCungGu_API.ViewModels;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class ChatsController : Controller
    {
        private readonly HttpClient _httpClient;
        private static Dictionary<string, WebSocket> _userWebSockets = new Dictionary<string, WebSocket>();

        public ChatsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Lấy danh sách các cuộc trò chuyện
        public async Task<IActionResult> Index()
        {
            string username = HttpContext.Session.GetString("TempUserName");
            ViewBag.CurrentUser = username;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ReportUser([FromBody] BaoCao reportRequest)
        {

            try
            {
                // Tạo nội dung body để gửi đến API
                var requestBody = new
                {
                    NguoiBaoCao = reportRequest.NguoiBaoCao,
                    DoiTuongBaoCao = reportRequest.DoiTuongBaoCao,
                    LyDoBaoCao = reportRequest.LyDoBaoCao,
                };

                // Gửi yêu cầu POST đến API
                var response = await _httpClient.PostAsJsonAsync("http://localhost:15172/api/chats/BaoCao", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<BlockUserResponse>(responseContent);
                    return Json(new
                    {
                        success = result?.Success,
                        message = result?.Message
                    });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, message = $"Lỗi khi báo cáo: {errorMessage}" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        public class CheckMatchResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<string> MatchedUsers { get; set; }
        }
        // GET: Profile/Details
        [HttpGet]
        public async Task<ActionResult> Profiles(string username)
        {

            // Gọi API để lấy thông tin profile
            var response = await _httpClient.GetAsync($"http://localhost:15172/api/Chats/Profiles?username={username}");

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var profile = JsonConvert.DeserializeObject<Profile>(content);

                return View(profile);
            }
            return BadRequest();
        }
        [HttpGet]
        public async Task<IActionResult> GetMessages(string receiverUsername)
        {
            string senderUsername = HttpContext.Session.GetString("TempUserName");

            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:15172/api/Chats?ReceiverUserName={receiverUsername}&SenderUsername={senderUsername}");

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var messages = JsonConvert.DeserializeObject<List<MessageVM>>(responseContent);

                return Json(new { success = true, messages = messages });
            }
            catch (Exception ex)
            {
                var errorResponse = new { success = true, message = "Gửi lời chào đến bạn mới!" };
                return Json(errorResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetConversation(string username)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:15172/api/Chats/CheckMatchSwipeAction?username={username}");
                response.EnsureSuccessStatusCode();

                // Đọc nội dung JSON từ phản hồi
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize JSON thành đối tượng CheckMatchResponse
                var result = JsonConvert.DeserializeObject<CheckMatchResponse>(jsonResponse);
                if (result.Success)
                {
                    foreach (string ReceiverUsername in result.MatchedUsers)
                    {
                        var requestBody = new
                        {
                            SenderUsername = username,
                            ReceiverUsername = ReceiverUsername
                        };

                        var startConversationResponse = await _httpClient.PostAsJsonAsync($"http://localhost:15172/api/Chats/StartConversation", requestBody);

                        if (!startConversationResponse.IsSuccessStatusCode)
                        {
                            var errorMessage = await startConversationResponse.Content.ReadAsStringAsync();
                            throw new Exception($"Failed to start conversation with {ReceiverUsername}. Status: {startConversationResponse.StatusCode}. Details: {errorMessage}");
                        }
                    }

                }
                // Call the API to get the list of conversations
                var responsDS = await _httpClient.GetAsync($"http://localhost:15172/api/Chats/Conversations?username={username}");
                response.EnsureSuccessStatusCode();

                var responseContent = await responsDS.Content.ReadAsStringAsync();
                var conversations = JsonConvert.DeserializeObject<List<ConversationVM>>(responseContent);
                return Json(new { success = true, conversations = conversations });
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", $"Error calling API: {ex.Message}");
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                return View();
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageVM message)
        {
            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            try
            {
                message.Timestamp = DateTime.UtcNow;

                // Gửi tin nhắn qua API (nếu cần thiết để lưu vào cơ sở dữ liệu)
                var response = await _httpClient.PostAsJsonAsync("http://localhost:15172/api/Chats", message);

                if (response.IsSuccessStatusCode)
                {
                    // Gửi tin nhắn qua WebSocket đến người nhận
                    await SendMessageToUser(message.ReceiverUsername, message.Content);

                    return Json(new
                    {
                        success = true,
                        message = "Tin nhắn đã được gửi.",
                        ReceiverUsername = message.ReceiverUsername,
                        SenderUsername = message.SenderUsername,
                        content = message.Content
                    });
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Gửi tin nhắn thất bại: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi xảy ra: {ex.Message}");
            }

            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ConnectWebSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var userName = HttpContext.Session.GetString("TempUserName");

                if (!string.IsNullOrEmpty(userName))
                {
                    // Kiểm tra xem người dùng đã có kết nối WebSocket hay chưa
                    if (_userWebSockets.ContainsKey(userName))
                    {
                        await _userWebSockets[userName].CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", CancellationToken.None);
                        _userWebSockets[userName] = webSocket;
                    }
                    else
                    {
                        _userWebSockets[userName] = webSocket;
                    }
                }
                else
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "TempUserName không có trong session", CancellationToken.None);
                    return BadRequest("TempUserName không có trong session.");
                }

                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        _userWebSockets.Remove(userName);
                    }
                    else
                    {
                        // Nhận tin nhắn từ WebSocket
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received message: {message}");

                        // Chuyển đối tượng JSON thành MessageVM
                        var jsonMessage = JsonConvert.DeserializeObject<MessageVM>(message);

                        // Lưu tin nhắn vào cơ sở dữ liệu thông qua API
                        await SaveMessageToDatabase(jsonMessage);

                        // Sau khi lưu, gửi lại tin nhắn cho người nhận qua WebSocket
                        await SendMessageToUser(jsonMessage.ReceiverUsername, jsonMessage.Content);
                    }
                }
            }
            return View();
        }

        public async Task SaveMessageToDatabase(MessageVM message)
        {
            try
            {
                // Gọi API để lưu tin nhắn vào cơ sở dữ liệu
                var response = await _httpClient.PostAsJsonAsync("http://localhost:15172/api/Chats", message);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Message saved to database.");
                }
                else
                {
                    Console.WriteLine($"Error saving message: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving message: {ex.Message}");
            }
        }



        public async Task SendMessageToUser(string receiverUsername, string messageContent)
        {
            if (_userWebSockets.ContainsKey(receiverUsername))
            {
                WebSocket receiverWebSocket = _userWebSockets[receiverUsername];
                if (receiverWebSocket.State == WebSocketState.Open)
                {
                    var userName = HttpContext.Session.GetString("TempUserName");
                    var message = new
                    {
                        SenderUsername = userName,
                        ReceiverUsername = receiverUsername,
                        Content = messageContent,
                        Timestamp = DateTime.UtcNow
                    };

                    var messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                    await receiverWebSocket.SendAsync(new ArraySegment<byte>(messageBytes, 0, messageBytes.Length),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest user)
        {
            string senderUsername = HttpContext.Session.GetString("TempUserName");
            string receiverUsername = user.ReceiverUsername;
            if (string.IsNullOrEmpty(senderUsername))
            {
                return Json(new { success = false, message = "Người dùng hiện tại không xác định." });
            }

            try
            {
                // Gửi yêu cầu tới API
                var requestBody = new
                {
                    ReceiverUserName = receiverUsername,
                    SenderUsername = senderUsername
                };

                var response = await _httpClient.PostAsJsonAsync("http://localhost:15172/api/Chats/BlockUser", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<BlockUserResponse>(responseContent);

                    return Json(new
                    {
                        success = result?.Success,
                        message = result?.Message
                    });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return Json(new
                    {
                        success = false,
                        message = $"Lỗi khi chặn người dùng: {errorMessage}"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Đã xảy ra lỗi: {ex.Message}"
                });
            }
        }
        public class BlockUserResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
        }
        public class BlockUserRequest
        {
            public string ReceiverUsername { get; set; }
            public string SenderUsername { get; set; }
        }
        [HttpPost]
        public async Task<IActionResult> UnblockUser([FromBody] BlockUserRequest user)
        {
            string receiverUsername = user.ReceiverUsername;
            string senderUsername = HttpContext.Session.GetString("TempUserName");

            if (string.IsNullOrEmpty(senderUsername))
            {
                return Json(new { success = false, message = "Người dùng hiện tại không xác định." });
            }

            try
            {
                // Gửi yêu cầu tới API
                var requestBody = new
                {
                    ReceiverUserName = receiverUsername,
                    SenderUsername = senderUsername
                };

                var response = await _httpClient.PostAsJsonAsync("http://localhost:15172/api/Chats/UnblockUser", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    return Json(new
                    {
                        success = true,
                        message = result?.Message ?? "Đã bỏ chặn người dùng thành công."
                    });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return Json(new
                    {
                        success = false,
                        message = $"Lỗi khi bỏ chặn người dùng: {errorMessage}"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Đã xảy ra lỗi: {ex.Message}"
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> CheckBlockStatus(string receiverUsername)
        {
            string senderUsername = HttpContext.Session.GetString("TempUserName");

            if (string.IsNullOrEmpty(senderUsername))
            {
                return Json(new { success = false, message = "Người dùng hiện tại không xác định." });
            }

            try
            {
                // Gửi yêu cầu tới API
                var response = await _httpClient.GetAsync($"http://localhost:15172/api/Chats/CheckBlockStatus?ReceiverUserName={receiverUsername}&SenderUsername={senderUsername}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CheckBlockStatusResponse>(responseContent);

                    // Trả về thông tin chi tiết về chiều của hành động chặn
                    return Json(new
                    {
                        success = true,
                        daChan = result.DaChan,
                        blockDirection = result.BlockDirection, // Trả về BlockDirection
                        message = result.Message
                    });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return Json(new
                    {
                        success = false,
                        message = $"Lỗi khi kiểm tra trạng thái chặn: {errorMessage}"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Đã xảy ra lỗi: {ex.Message}"
                });
            }
        }

    }
    public class CheckBlockStatusResponse
    {
        public bool Success { get; set; }
        public bool DaChan { get; set; }
        public string Message { get; set; }
        public string BlockDirection { get; set; }
    }
}
