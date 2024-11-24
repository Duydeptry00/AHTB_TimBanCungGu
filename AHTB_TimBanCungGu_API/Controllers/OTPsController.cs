using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email không được để trống.");

            // Tạo OTP ngẫu nhiên (gồm 6 số)
            var otp = new Random().Next(100000, 999999).ToString();

            // Lưu OTP vào cache với thời gian hết hạn 5 phút (sử dụng _cache hoặc cách lưu trữ khác)
            var expirationTime = DateTimeOffset.Now.AddMinutes(5);
            _cache.Set(email, otp, expirationTime);

            // Tạo token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("AHTB_DATN"); // Thay bằng khóa bí mật của bạn
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim("email", email),
            new Claim("otp", otp) // Bao gồm OTP trong payload
        }),
                Expires = DateTime.UtcNow.AddMinutes(5), // Token có thời hạn 5 phút
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            // Gửi OTP qua email
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("sukanephan@gmail.com", "ndth xcmu baef vozo"), // Lưu ý bảo mật mật khẩu
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("gamekhang990@gmail.com"),
                    Subject = "Mã OTP và Token của bạn",
                    Body = $"Mã OTP của bạn là: {otp}. Mã này sẽ hết hạn sau 5 phút.\n\nToken của bạn là: {jwtToken}",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);

                return Ok(new { Message = "OTP đã được gửi qua email.", Token = jwtToken });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi gửi email: {ex.Message}");
            }
        }

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
                    // OTP khớp, xóa token và email khỏi cache
                    _cache.Remove(request.Email);
                    _cache.Remove(request.Otp);

                    // Xác nhận OTP thành công
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
