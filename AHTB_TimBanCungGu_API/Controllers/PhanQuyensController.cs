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
            // Kiểm tra xem người dùng có tồn tại trong cơ sở dữ liệu hay không
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userRole.Username);
            if (user == null)
            {
                return BadRequest("Người dùng không hợp lệ.");
            }

            // Kiểm tra xem vai trò có tồn tại trong cơ sở dữ liệu hay không
            var roleExists = await _context.Quyen.AnyAsync(r => r.IDRole == userRole.Id_Role);
            if (!roleExists)
            {
                return BadRequest("Vai trò không hợp lệ.");
            }

            // Tạo đối tượng mới cho User_Role và gán các giá trị cần thiết
            var newUserRole = new User_Role
            {
                IDRole = userRole.Id_Role,          // Gán ID vai trò từ request
                UsID = user.UsID,                    // Lấy UsID từ đối tượng User tìm được
                TenRole = userRole.Tenrole          // Tên vai trò từ request
            };

            // Thêm đối tượng vào bảng User_Role
            _context.Role.Add(newUserRole);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

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
