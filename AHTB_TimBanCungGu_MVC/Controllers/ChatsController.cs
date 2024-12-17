using Microsoft.AspNetCore.Mvc;
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
using System.Linq;
using AHTB_TimBanCungGu_API.Models;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class ChatsController : Controller
    {
        private readonly HttpClient _httpClient;
        // Lưu trữ WebSocket kết nối cho các phiên chức năng trong nhắn tin
        private static Dictionary<string, WebSocket> _userWebSockets = new Dictionary<string, WebSocket>();
        private static Dictionary<string, Dictionary<string, (WebSocket, string)>> _movieSessionWebSockets = new Dictionary<string, Dictionary<string, (WebSocket, string)>>();
        // Khai báo danh sách các phiên xem phim cùng
        private static List<MovieSession> _sessions = new List<MovieSession>();
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
        public async Task<IActionResult> WatchTogether(string senderUsername, string receiverUsername, string PhimSeXem, string idPhim)
        {
            if (string.IsNullOrEmpty(senderUsername))
            {
                // Nếu không có người dùng hiện tại trong session, chuyển hướng về trang đăng nhập
                return RedirectToAction("index", "chats");
            }

            // Kiểm tra nếu phiên đã tồn tại hoặc tạo mới
            var session = _sessions.FirstOrDefault(s => s.Users.Contains(senderUsername) && s.Users.Contains(receiverUsername));
            if (session == null)
            {
                // Nếu PhimSeXem không được chỉ định, lấy phim mặc định (ví dụ: "Sample Movie")
                string movieTitle = string.IsNullOrEmpty(PhimSeXem) ? "Sample Movie" : PhimSeXem;

                // Gọi API để lấy URL phim từ IdPhim
                var movieUrl = await MapMovieTitleToUrl(idPhim); // Đảm bảo MapMovieTitleToUrl trả về đúng URL

                // Tạo một phiên mới nếu chưa có
                session = new MovieSession
                {
                    SessionId = Guid.NewGuid().ToString(),
                    MovieTitle = movieTitle, // Gán tên bộ phim
                    MovieUrl = movieUrl,     // Gán URL phim
                    Users = new List<string> { senderUsername, receiverUsername },
                    CurrentTime = 0, // Thời gian phim hiện tại
                    IsPlaying = false // Trạng thái phát phim
                };
                _sessions.Add(session);
            }
            else
            {
                // Nếu phim đã được chọn, cập nhật lại phim trong phiên
                if (!string.IsNullOrEmpty(idPhim))
                {
                    var movieUrl = await MapMovieTitleToUrl(idPhim); // Lấy URL phim mới nếu có thay đổi
                    session.MovieTitle = PhimSeXem;
                    session.MovieUrl = movieUrl;
                }
            }

            // Lấy danh sách tất cả bộ phim từ API sử dụng _httpClient
            List<Phim> danhSachPhim = new List<Phim>();
            var response = await _httpClient.GetAsync("http://localhost:15172/api/XemPhimCungs/GetAllPhim");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                danhSachPhim = JsonConvert.DeserializeObject<List<Phim>>(result);
            }

            // Truyền thông tin vào view
            ViewBag.DanhSachPhim = danhSachPhim;
            ViewBag.CurrentUser = senderUsername;
            ViewBag.ReceiverUser = receiverUsername;
            ViewBag.SessionId = session.SessionId; // Truyền sessionId vào view
            ViewBag.MovieTitle = session.MovieTitle;
            ViewBag.MovieUrl = session.MovieUrl;

            return View(session);
        }

        public async Task<IActionResult> GetMovies()
        {
            // URL của API
            string apiUrl = "http://localhost:15172/api/XemPhimCungs/GetAllPhim";

            // Gửi yêu cầu GET đến API và lấy dữ liệu
            var response = await _httpClient.GetStringAsync(apiUrl);

            // Chuyển đổi dữ liệu JSON trả về thành danh sách các phim
            var phimList = JsonConvert.DeserializeObject<List<Phim>>(response);

            // Trả về dữ liệu dưới dạng JSON
            return Json(phimList);
        }
        public async Task<string> MapMovieTitleToUrl(string movieTitle)
        {
            // URL của API cần gọi
            string url = $"http://localhost:15172/api/XemPhimCungs/GetPhim?idPhim={movieTitle}"; // Sử dụng movieTitle thay vì idPhim

            // Gọi API và lấy dữ liệu trả về
            var response = await _httpClient.GetAsync(url);

            // Kiểm tra nếu API trả về thành công (status code 200)
            if (response.IsSuccessStatusCode)
            {
                // Đọc nội dung trả về từ API dưới dạng chuỗi
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize chuỗi JSON thành đối tượng cần thiết
                var movieData = JsonConvert.DeserializeObject<MovieDetails>(jsonResponse);

                // Thêm vào đường dẫn cơ sở của nhà cung cấp dịch vụ xem phim
                string baseUrl = "https://vidsrc.cc/v2/embed/movie/";

                // Kết hợp thành URL hoàn chỉnh
                return $"{baseUrl}{movieData.SourcePhim}?autoPlay=false"; // Dùng SourcePhim từ movieData để xây dựng URL
            }

            // Trả về một chuỗi trống hoặc thông báo lỗi nếu không thành công
            return string.Empty;
        }
        public class MovieDetails
        {
            public string TenPhim { get; set; }
            public string SourcePhim { get; set; }
            public string Premium { get; set; }
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
                        else if (parsedMessage != null && parsedMessage.type == "movieInvite")
                        {
                            // Nhận mời xem phim
                            string senderUsername = parsedMessage.senderUsername;
                            string receiverUsername = parsedMessage.receiverUsername;
                            string movieName = parsedMessage.movieName;
                            string idPhim = parsedMessage.movieId;

                            // Tạo thông báo mời xem phim
                            var movieInviteMessage = new
                            {
                                type = "movieInvite",
                                message = $"{senderUsername} mời bạn xem phim '{movieName}' cùng.",
                                movieName = movieName,
                                senderUsername = senderUsername,
                                receiverUsername = receiverUsername,
                                idPhim = idPhim
                            };

                            // Gửi lời mời tới người nhận nếu có kết nối WebSocket
                            if (_userWebSockets.TryGetValue(receiverUsername, out WebSocket receiverSocket))
                            {
                                await receiverSocket.SendAsync(
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(movieInviteMessage))),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None
                                );
                            }
                        }
                        else if (parsedMessage != null && parsedMessage.type == "block")
                        {
                            string senderUsername = parsedMessage.senderUsername;
                            string receiverUsername = parsedMessage.receiverUsername;

                            // Gọi hàm để thực hiện hành động chặn
                            var isBlocked = await BlockUser(senderUsername, receiverUsername);

                            // Gửi phản hồi tới người gửi (người thực hiện chặn)
                            var responseToSender = new
                            {
                                type = "blockResponse",
                                success = isBlocked,
                                message = isBlocked ? "User has been blocked successfully." : "Failed to block the user."
                            };

                            await webSocket.SendAsync(
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseToSender))),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None
                            );

                            // Nếu chặn thành công, gửi thông báo tới người bị chặn (receiver)
                            if (isBlocked && _userWebSockets.TryGetValue(receiverUsername, out WebSocket receiverSocket))
                            {
                                var notificationToReceiver = new
                                {
                                    type = "blockNotification",
                                    message = $"{senderUsername} has blocked you.",
                                    senderUsername = senderUsername
                                };

                                await receiverSocket.SendAsync(
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(notificationToReceiver))),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None
                                );
                            }
                        }
                        else if (parsedMessage != null && parsedMessage.type == "unblock")
                        {
                            // Lấy thông tin người gửi và người nhận
                            string senderUsername = parsedMessage.senderUsername;
                            string receiverUsername = parsedMessage.receiverUsername;

                            // Gọi hàm để thực hiện hành động hủy chặn
                            var isUnblocked = await UnblockUser(senderUsername, receiverUsername);

                            // Gửi phản hồi cho client về kết quả hủy chặn
                            var response = new
                            {
                                type = "unblockResponse",
                                success = isUnblocked,
                                message = isUnblocked ? "User has been unblocked successfully." : "Failed to unblock the user."
                            };

                            await webSocket.SendAsync(
                                new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response))),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None
                            );

                            // Nếu hủy chặn thành công, gửi thông báo tới người bị hủy chặn (receiver)
                            if (isUnblocked && _userWebSockets.TryGetValue(receiverUsername, out WebSocket receiverSocket))
                            {
                                var notificationToReceiver = new
                                {
                                    type = "unblockNotification",
                                    message = $"{senderUsername} has unblocked you.",
                                    senderUsername = senderUsername
                                };

                                await receiverSocket.SendAsync(
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(notificationToReceiver))),
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None
                                );
                            }
                        }

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
        [HttpGet]
        public async Task<IActionResult> ConnectMovieSession(string sessionId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var userName = HttpContext.Session.GetString("TempUserName");

                if (!string.IsNullOrEmpty(userName))
                {
                    // Kiểm tra số lượng người tham gia trong phiên
                    if (_movieSessionWebSockets.ContainsKey(sessionId))
                    {
                        var participants = _movieSessionWebSockets[sessionId];
                        if (participants.Count >= 2)
                        {
                            // Nếu phiên đã đầy (2 người), từ chối kết nối
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Movie session is full", CancellationToken.None);
                            return BadRequest("The movie session is already full.");
                        }
                        else
                        {
                            // Sử dụng tuple (WebSocket, string)
                            _movieSessionWebSockets[sessionId].Add(userName, (webSocket, userName));
                        }
                    }
                    else
                    {
                        // Nếu chưa có phiên, tạo phiên mới và thêm người dùng vào
                        _movieSessionWebSockets[sessionId] = new Dictionary<string, (WebSocket, string)>
                        {
                            { userName, (webSocket, userName) }
                        };
                    }
                }
                else
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "TempUserName không có trong session", CancellationToken.None);
                    return BadRequest("TempUserName không có trong session.");
                }

                // Lắng nghe các thông điệp từ người dùng
                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        // Xử lý ngắt kết nối và loại bỏ người dùng khỏi phiên
                        if (_movieSessionWebSockets.ContainsKey(sessionId))
                        {
                            _movieSessionWebSockets[sessionId].Remove(userName);
                            // Nếu phiên không còn người tham gia, có thể xóa session khỏi dictionary
                            if (_movieSessionWebSockets[sessionId].Count == 0)
                            {
                                _movieSessionWebSockets.Remove(sessionId);
                            }
                        }
                    }
                    else
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received message: {message}");

                        // Xử lý các loại tin nhắn
                        var parsedMessage = JsonConvert.DeserializeObject<dynamic>(message);

                        if (parsedMessage != null && parsedMessage.type == "playPause")
                        {
                            // Thực hiện hành động play hoặc pause
                            string action = parsedMessage.action; // play hoặc pause
                            await SyncMovieAction(sessionId, action);
                        }
                        else if (parsedMessage != null && parsedMessage.type == "seek")
                        {
                            // Thực hiện hành động seek đến vị trí mới
                            int seekTime = parsedMessage.seekTime;
                            await SyncSeekTime(sessionId, seekTime);
                        }
                    }
                }
            }
            return View();
        }
        private async Task SyncMovieAction(string sessionId, string action)
        {
            // Lặp qua tất cả người tham gia trong phiên và gửi hành động đến họ
            if (_movieSessionWebSockets.ContainsKey(sessionId))
            {
                foreach (var participant in _movieSessionWebSockets[sessionId].Values)
                {
                    // participant.Item1 là WebSocket, participant.Item2 là tên người dùng (string)
                    var responseMessage = new { type = "playPause", action = action };
                    var message = JsonConvert.SerializeObject(responseMessage);
                    var buffer = Encoding.UTF8.GetBytes(message);
                    await participant.Item1.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
        private async Task SyncSeekTime(string sessionId, int seekTime)
        {
            // Lặp qua tất cả người tham gia trong phiên và gửi thời gian seek đến họ
            if (_movieSessionWebSockets.ContainsKey(sessionId))
            {
                foreach (var participant in _movieSessionWebSockets[sessionId].Values)
                {
                    // participant.Item1 là WebSocket, participant.Item2 là tên người dùng (string)
                    var responseMessage = new { type = "seek", seekTime = seekTime };
                    var message = JsonConvert.SerializeObject(responseMessage);
                    var buffer = Encoding.UTF8.GetBytes(message);
                    await participant.Item1.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
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
        [HttpGet]
        public IActionResult CheckIfUserOnline(string username)
        {
            bool isOnline = _userWebSockets.ContainsKey(username); // Kiểm tra người dùng có kết nối WebSocket không
            return Json(new { isOnline });
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
