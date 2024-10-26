using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OTPsController : ControllerBase
    {
        private readonly IMemoryCache _cache;

        public OTPsController(IMemoryCache cache)
        {
            _cache = cache;
        }

        // Gửi OTP qua email
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email không được để trống.");

            // Tạo OTP ngẫu nhiên (gồm 6 số)
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP vào cache với thời gian hết hạn 5 phút
            var expirationTime = DateTimeOffset.Now.AddMinutes(5);
            _cache.Set(email, otp, expirationTime);

            // Gửi OTP qua email
            try
            {
                // Sử dụng SMTP client với thông tin của Gmail hoặc dịch vụ email của bạn
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587, // Gmail SMTP port cho TLS
                    Credentials = new NetworkCredential("sukanephan@gmail.com", "ndth xcmu baef vozo"), // Sử dụng mật khẩu ứng dụng Gmail
                    EnableSsl = true, // Bật SSL cho bảo mật
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("gamekhang990@gmail.com"), // Thay bằng địa chỉ email gửi
                    Subject = "Mã OTP của bạn",
                    Body = $"Mã OTP của bạn là: {otp}. Mã này sẽ hết hạn sau 5 phút.",
                    IsBodyHtml = false, // Bạn có thể set là true nếu muốn gửi email dạng HTML
                };
                mailMessage.To.Add(email); // Thêm email người nhận

                // Gửi email không đồng bộ
                await smtpClient.SendMailAsync(mailMessage);

                return Ok("OTP đã được gửi qua email.");
            }
            catch (Exception ex)
            {
                // Bắt lỗi và trả về trạng thái HTTP 500 nếu có lỗi xảy ra
                return StatusCode(500, $"Lỗi khi gửi email: {ex.Message}");
            }
        }

        // Xác nhận OTP
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Otp))
                return BadRequest("Email hoặc OTP không được để trống.");

            // Kiểm tra OTP có trong cache không
            if (_cache.TryGetValue(request.Email, out string savedOtp))
            {
                if (savedOtp == request.Otp)
                {
                    // OTP khớp
                    return Ok("OTP xác nhận thành công.");
                }
                else
                {
                    // OTP không đúng
                    return BadRequest("OTP không hợp lệ.");
                }
            }

            // OTP hết hạn hoặc không tồn tại
            return BadRequest("OTP đã hết hạn hoặc không tồn tại.");
        }
    }

    // Lớp dùng cho việc xác thực OTP
    public class OtpVerificationRequest
    {
        public string Email { get; set; }
        public string Otp { get; set; }
    }
}
