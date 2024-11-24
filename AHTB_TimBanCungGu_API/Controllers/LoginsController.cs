using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Authorization;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginsController : ControllerBase
    {
        private readonly DBAHTBContext _context;
        private const string SecretKey = "kF9lQ4!gM62v@RzYtC7z1wX2JpHp7B5z"; // Thay bằng key của bạn
        private const string Issuer = "Admin"; // Cung cấp Issuer

        public LoginsController(DBAHTBContext context)
        {
            _context = context;
        }

        // User Registration
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Email))
                return BadRequest("Username, password, và email không để trống!");

            // Kiểm tra nếu người dùng đã tồn tại
            var existingUser = await _context.Users
                .Include(u => u.ThongTinCN)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName ||
                                           u.ThongTinCN.Email == request.Email);

            if (existingUser != null)
                return BadRequest("Username hoặc email đã tồn tại");

            // Mã hóa mật khẩu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Tạo mới người dùng
            var newUser = new User
            {
                UsID = Guid.NewGuid().ToString(),
                UserName = request.UserName,
                Password = hashedPassword,
                TrangThai = "Hoạt Động"
            };

            var newProfile = new ThongTinCaNhan
            {
                UsID = newUser.UsID,
                Email = request.Email,
                HoTen = "",
                GioiTinh = "",
                DiaChi = "",
                NgaySinh = DateTime.Now,
                SoDienThoai = "",
                IsPremium = false,
                MoTa = "",
                NgayTao = DateTime.Now,
                TrangThai = "Hoạt Động"
            };

            _context.Users.Add(newUser);
            _context.ThongTinCN.Add(newProfile);
            await _context.SaveChangesAsync();

            return Ok("User đăng ký thành công.");
        }

        // User Login và tạo JWT
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            request.UserType = "khach"; // Mặc định là khách nếu không có quyền

            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username và password không để trống.");

            // Tìm người dùng theo tên người dùng hoặc email
            var user = await _context.Users
                .Include(u => u.ThongTinCN)
                .FirstOrDefaultAsync(u => u.UserName == request.UserName ||
                                          u.ThongTinCN.Email == request.UserName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized("Thông tin đăng nhập không hợp lệ.");

            // Kiểm tra xem người dùng có vai trò "nhanvien" hoặc "Admin" trong bảng User_Role hay không
            var userRole = await _context.Role
                .Include(ur => ur.Role) // Bao gồm thông tin bảng Role nếu cần
                .FirstOrDefaultAsync(u => u.UsID == user.UsID && u.Role.TenRole == "Admin" || u.Role.TenRole == "Nhân Viên");

            if (userRole != null)
            {
                request.UserType = userRole.Role.TenRole; // Thiết lập UserType theo vai trò tìm thấy
            }

            // Tạo JWT token nếu người dùng hợp lệ
            var expiration = DateTime.UtcNow.AddMinutes(30); // Hết hạn token sau 30 phút
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserType", request.UserType) // Thêm loại người dùng vào token
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Issuer,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenString, expiration = expiration , userType = request.UserType });
        }

    }

    public class UserRegisterRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

    }

    public class UserLoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
    }
}
