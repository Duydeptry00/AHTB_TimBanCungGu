using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginsController : ControllerBase
    {
        private readonly DBAHTBContext _context;

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

            // Check if the user already exists by username or email
            var existingUser = await _context.Users
                .Include(u => u.ThongTinCN) // Include to access the email
                .FirstOrDefaultAsync(u => u.UserName == request.UserName ||
                                           u.ThongTinCN.Email == request.Email);

            if (existingUser != null)
                return BadRequest("Username hoặc email đã tồn tại");

            // Hash the user's password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user entity
            var newUser = new User
            {
                UsID = Guid.NewGuid().ToString(),
                UserName = request.UserName,
                Password = hashedPassword,
                TrangThai = "Hoạt Động" // Status field
            };

            // Create new personal information entity
            var newProfile = new ThongTinCaNhan
            {
                UsID = newUser.UsID,
                Email = request.Email, // Set the email here
                HoTen = "", // Initialize other properties as needed
                GioiTinh = "",
                NgaySinh = DateTime.Now, // Or set to a default date
                DiaChi = "",
                SoDienThoai = "",
                IsPremium = false,
                MoTa = "",
                NgayTao = DateTime.Now,
                TrangThai = "Hoạt Động"
            };

            // Add to the database
            _context.Users.Add(newUser);
            _context.ThongTinCN.Add(newProfile); // Assuming you have a DbSet<ThongTinCaNhan> in your context
            await _context.SaveChangesAsync();

            return Ok("User đăng ký thành công.");
        }

        // User Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username và password không để trống.");

            // Find user by username or email
            var user = await _context.Users
                .Include(u => u.ThongTinCN) // Include to access the email
                .FirstOrDefaultAsync(u => u.UserName == request.UserName ||
                                          u.ThongTinCN.Email == request.UserName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized("Invalid credentials.");

            // Return success message instead of JWT
            return Ok("Đăng nhập thành công.");
        }
    }

    public class UserRegisterRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; } // Added Email property
    }

    public class UserLoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
