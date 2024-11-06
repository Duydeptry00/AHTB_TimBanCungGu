using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using BCrypt.Net;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // Sử dụng ConcurrentDictionary cho lưu trữ token và thời gian tạo tạm thời, thread-safe
        private static readonly ConcurrentDictionary<string, DateTime> TokenStorage = new();
        private readonly DBAHTBContext _context;

        public AccountController(DBAHTBContext context)
        {
            _context = context;
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // Kiểm tra xem email có hợp lệ không
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest("Email không hợp lệ.");
            }

            // Giả sử email đã được xác thực là tồn tại trong cơ sở dữ liệu
            string token = Guid.NewGuid().ToString(); // Tạo token
            TokenStorage[token] = DateTime.UtcNow; // Lưu token với thời gian tạo

            var baseUrl = request.Local;
            var callbackUrl = $"{baseUrl}/LoginvsRegister/DoiMatKhau?token={token}&email={request.Email}";

            // Gửi email đường link đặt lại mật khẩu
            var emailSent = await SendEmailAsync(request.Email, callbackUrl);

            if (!emailSent)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "Có lỗi xảy ra khi gửi email.");
            }

            return Ok("Đường link đặt lại mật khẩu đã được gửi tới email của bạn.");
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromBody] tokens token)
        {
            if (TokenStorage.ContainsKey(token.token))
            {
                // Kiểm tra thời gian hết hạn
                if (DateTime.UtcNow - TokenStorage[token.token] <= TimeSpan.FromHours(1))
                {
                    // Token hợp lệ, cho phép người dùng đặt lại mật khẩu
                    return Ok("Token hợp lệ. Bạn có thể đặt lại mật khẩu.");
                }
                else
                {
                    // Token hết hạn
                    return BadRequest("Token hết hạn.");
                }
            }
            else
            {
                // Token không hợp lệ
                return BadRequest("Token không hợp lệ.");
            }
        }

        private async Task<bool> SendEmailAsync(string email, string callbackUrl)
        {
            var message = new MailMessage
            {
                From = new MailAddress("sukanephan@gmail.com"), // Địa chỉ email người gửi
                Subject = "Đặt lại mật khẩu",
                Body = $"Vui lòng nhấp vào đường link dưới đây để đặt lại mật khẩu của bạn:<br/><a href='{callbackUrl}'>Đặt lại mật khẩu</a><br/>Nếu bạn không yêu cầu đặt lại mật khẩu, bạn có thể bỏ qua email này.",
                IsBodyHtml = true
            };

            message.To.Add(email); // Địa chỉ email người nhận

            // Sử dụng cấu hình của Gmail
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587, // Gmail SMTP port cho TLS
                Credentials = new NetworkCredential("sukanephan@gmail.com", "ndth xcmu baef vozo"), // Sử dụng mật khẩu ứng dụng Gmail
                EnableSsl = true // Bật SSL cho bảo mật
            };

            try
            {
                await smtpClient.SendMailAsync(message); // Gửi email
                return true; // Gửi thành công
            }
            catch (Exception ex)
            {
                // Ghi lại hoặc hiển thị thông tin lỗi
                Console.WriteLine($"Có lỗi xảy ra khi gửi email: {ex.Message}");
                return false; // Gửi không thành công
            }
        }
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] changepass request)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(request.email) || string.IsNullOrEmpty(request.newpassword))
            {
                return BadRequest("Email hoặc mật khẩu không hợp lệ.");
            }

            // Tìm người dùng theo email
            var user = await _context.Users.FirstOrDefaultAsync(e => e.ThongTinCN.Email == request.email);
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Mã hóa mật khẩu
            user.Password = HashPassword(request.email);

            // Lưu thay đổi vào cơ sở dữ liệu
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Ok("Mật khẩu đã được đổi thành công.");
            }
            else
            {
                return BadRequest("Có lỗi xảy ra khi đổi mật khẩu.");
            }
        }
        private string HashPassword(string password)
        {
            // Sử dụng BCrypt để mã hóa mật khẩu
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }



    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
        public string Local { get; set; }
        public string Password { get; set; }
        public string token { get; set; }
    }
    public class tokens
    {
        public string token { get; set; }
    }
    public class changepass
    {
        public string email { get; set; }
        public string newpassword { set; get; }
    }
}
