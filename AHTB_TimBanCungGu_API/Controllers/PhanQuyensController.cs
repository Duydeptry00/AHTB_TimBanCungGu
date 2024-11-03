using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.ViewModels;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhanQuyensController : ControllerBase
    {
        private readonly DBAHTBContext _context;

        public PhanQuyensController(DBAHTBContext context)
        {
            _context = context;
        }

        // POST: api/PhanQuyens
        [HttpPost]
        public async Task<ActionResult<User_role>> PostUserRole(User_role userRole)
        {
            // Kiểm tra xem người dùng và vai trò có tồn tại trong cơ sở dữ liệu không
            var userExists = await _context.Users.AnyAsync(u => u.UsID == userRole.Id_User); // Giả sử bạn có một bảng Users
            var roleExists = await _context.Quyen.AnyAsync(r => r.IDRole == userRole.Id_Role); // Giả sử bạn có một bảng Roles
            var newUserRole = new User_Role
            {
                IDRole = userRole.Id_Role,
                UsID = userRole.Id_User,
                TenRole = userRole.Tenrole
            };
            if (!userExists || !roleExists)
            {
                return BadRequest("Người dùng hoặc vai trò không hợp lệ.");
            }

            _context.Role.Add(newUserRole); // Thêm bản ghi mới vào User_Role
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu

            // Trả về kết quả sau khi thêm thành công
            return CreatedAtAction(nameof(GetUserRole), new { id = userRole.Id_Role }, userRole);
        }

        // GET: api/PhanQuyens/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<User_Role>> GetUserRole(int id)
        {
            var userRole = await _context.Role.FindAsync(id);

            if (userRole == null)
            {
                return NotFound();
            }

            return userRole;
        }
    }
}
