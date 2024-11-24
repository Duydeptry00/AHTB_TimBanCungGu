using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.ViewModels;
using System.Linq;

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

            // Kiểm tra xem nhân viên đó đã được cấp quyền đó trong cơ sở dữ liệu hay không
            var User_roleExists = await _context.Role.AnyAsync(r => r.IDRole == userRole.Id_Role && r.UsID == user.UsID);

            if (User_roleExists) // Nếu đã tồn tại, trả về lỗi
            {
                return BadRequest("Vai trò và người dùng đã tồn tại.");
            }


            // Tạo đối tượng mới cho User_Role và gán các giá trị cần thiết
            var newUserRole = new User_Role
            {
                IDRole = userRole.Id_Role,          // Gán ID vai trò từ request
                UsID = user.UsID,               // Lấy UsID từ đối tượng User tìm được
                TrangThai = userRole.TrangThai,
            };

            // Thêm đối tượng vào bảng User_Role
            _context.Role.Add(newUserRole);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về kết quả sau khi thêm thành công
            return CreatedAtAction(nameof(GetUserRole), new { id = userRole.Id_Role }, userRole);
        }
        [HttpGet]
        public async Task<ActionResult<ListPhanQuyen>> GetPhanQUyen()
        {
            var User_role = await _context.Role.Include(r => r.Role).Include(u => u.User).ToListAsync();
            var PhanQuyen = User_role.Select(PQ => new ListPhanQuyen
            {
                Id = PQ.IDRole_US,
                Module = PQ.Role.Module,
                Add = PQ.Role.Add,
                Update = PQ.Role.Update,
                Delete = PQ.Role.Delete,
                ReviewDetails = PQ.Role.ReviewDetails,
                Username = PQ.User.UserName,
                Tenrole = PQ.Role.TenRole,
            });
            return Ok(PhanQuyen);
        }
        // DELETE: api/PhanQuyens/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            // Find the User_Role by ID
            var userRole = await _context.Role.FindAsync(id);
            if (userRole == null)
            {
                return NotFound();  // Return 404 if the User_Role is not found
            }

            // Remove the User_Role from the context
            _context.Role.Remove(userRole);

            // Save changes to commit the deletion
            await _context.SaveChangesAsync();

            // Return a no-content status to indicate success
            return NoContent();
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
