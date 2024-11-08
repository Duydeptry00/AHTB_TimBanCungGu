using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System;
using AHTB_TimBanCungGu_MVC.Models;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class NhanTinController : Controller
    {
        private static Dictionary<string, WebSocket> _userWebSockets = new Dictionary<string, WebSocket>();
        private readonly IMongoCollection<BsonDocument> _messages;

        public NhanTinController()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("AHTBdb");
            _messages = database.GetCollection<BsonDocument>("Nhantin");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConnectMessageWebSocket()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var userName = HttpContext.Session.GetString("TempUserName");

                if (!string.IsNullOrEmpty(userName))
                {
                    _userWebSockets[userName] = webSocket;
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

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await HandleReceivedMessage(userName, message);
                    }
                }

                _userWebSockets.Remove(userName);
            }
            return BadRequest();
        }

        private async Task HandleReceivedMessage(string senderUserName, string messageJson)
        {
            var messageData = JsonConvert.DeserializeObject<Message>(messageJson);
            var receiverUserName = messageData.ReceiverUserName;

            if (_userWebSockets.TryGetValue(receiverUserName, out var receiverSocket) && receiverSocket.State == WebSocketState.Open)
            {
                var buffer = Encoding.UTF8.GetBytes(messageJson);
                var segment = new ArraySegment<byte>(buffer);

                try
                {
                    await receiverSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                    Console.WriteLine($"Message sent to {receiverUserName}: {messageData.Content}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to {receiverUserName}: {ex.Message}");
                }
            }

            // Lưu tin nhắn vào MongoDB
            var notification = new BsonDocument
            {
                { "Sender", senderUserName },
                { "Receiver", receiverUserName },
                { "Message", messageData.Content },
                { "Timestamp", DateTime.Now }
            };
            await _messages.InsertOneAsync(notification);
        }
    }
}
