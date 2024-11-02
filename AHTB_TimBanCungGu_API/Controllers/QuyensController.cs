using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AHTB_TimBanCungGu_API.ViewModels;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuyensController : ControllerBase
    {
        private readonly DBAHTBContext _context;

        public QuyensController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: api/Quyens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleVM>>> GetRoles()
        {
            // Truy xuất danh sách Role từ cơ sở dữ liệu, bao gồm dữ liệu liên kết qua User_Role
            var roles = await _context.Quyen
                .Include(role => role.User_Role) // Bao gồm User_Role như là bảng trung gian
                .ThenInclude(ur => ur.User)      // Sau đó bao gồm thông tin User từ User_Role
                .ToListAsync();

            // Chuyển đổi mỗi Role thành RoleVM
            var roleVMs = roles.Select(role => new RoleVM
            {
                IDRole = role.IDRole,
                Module = role.Module,
                Add = role.Add,
                Update = role.Update,
                Delete = role.Delete,
                ReviewDetails = role.ReviewDetails,
                User = role.User_Role.Any()
                    ? string.Join(", ", role.User_Role.Select(ur => ur.User.UserName)) // Nếu có người dùng, lấy tên của họ
                    : "Chưa có nhân viên nào nhận quyền" // Nếu không có, hiển thị thông báo
            }).ToList();

            return Ok(roleVMs);
        }
        // GET: api/Quyens/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleVM>> GetRoleById(int id)
        {
            // Truy xuất quyền theo ID từ cơ sở dữ liệu
            var role = await _context.Quyen
                .Include(r => r.User_Role) // Bao gồm User_Role như là bảng trung gian
                .ThenInclude(ur => ur.User)  // Sau đó bao gồm thông tin User từ User_Role
                .FirstOrDefaultAsync(r => r.IDRole == id); // Lấy quyền có ID tương ứng

            if (role == null)
            {
                return NotFound(); // Nếu không tìm thấy quyền, trả về 404
            }

            // Chuyển đổi Role thành RoleVM
            var roleVM = new RoleVM
            {
                IDRole = role.IDRole,
                Module = role.Module,
                Add = role.Add,
                Update = role.Update,
                Delete = role.Delete,
                ReviewDetails = role.ReviewDetails,
                User = role.User_Role.Any()
                    ? string.Join(", ", role.User_Role.Select(ur => ur.User.UserName)) // Nếu có người dùng, lấy tên của họ
                    : "Chưa có nhân viên nào nhận quyền" // Nếu không có, hiển thị thông báo
            };

            return Ok(roleVM); // Trả về thông tin quyền
        }


        [HttpPost]
        public async Task<ActionResult<Role>> CreateRole(RoleVM roleVM)
        {
            // Kiểm tra roleVM có null không
            if (roleVM == null)
            {
                return BadRequest("Quyền đang rỗng.");
            }

            // Kiểm tra quyền có trùng lặp không dựa trên Module
            if (_context.Quyen.Any(r => r.Module == roleVM.Module))
            {
                return Conflict("Quyền với module này đã tồn tại.");
            }

            // Tạo đối tượng Role mới từ RoleVM
            var quyen = new Role
            {
                Module = roleVM.Module,
                Add = roleVM.Add,
                Update = roleVM.Update,
                Delete = roleVM.Delete,
                ReviewDetails = roleVM.ReviewDetails
            };

            // Thêm quyền mới vào cơ sở dữ liệu
            _context.Quyen.Add(quyen);
            await _context.SaveChangesAsync();

            // Trả về quyền vừa được thêm vào
            return CreatedAtAction(nameof(GetRoles), new { id = quyen.IDRole }, quyen);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, RoleVM roleVM)
        {
            if (roleVM == null)
            {
                return BadRequest("Dữ liệu quyền không hợp lệ.");
            }

            // Tìm quyền hiện có trong cơ sở dữ liệu theo ID
            var existingRole = await _context.Quyen.FindAsync(id);
            if (existingRole == null)
            {
                return NotFound("Không tìm thấy quyền.");
            }

            // Kiểm tra trùng lặp module trong cơ sở dữ liệu khi cập nhật
            if (_context.Quyen.Any(r => r.Module == roleVM.Module && r.IDRole != id))
            {
                return Conflict("Quyền với module này đã tồn tại.");
            }

            // Cập nhật các thuộc tính của đối tượng Role từ RoleVM
            existingRole.Module = roleVM.Module;
            existingRole.Add = roleVM.Add;
            existingRole.Update = roleVM.Update;
            existingRole.Delete = roleVM.Delete;
            existingRole.ReviewDetails = roleVM.ReviewDetails;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
                {
                    return NotFound("Quyền không tồn tại sau khi cập nhật.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        // DELETE: api/Quyens/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Quyen.FindAsync(id);
            if (role == null)
            {
                return NotFound("Không tìm thấy quyền cần xóa.");
            }

            _context.Quyen.Remove(role);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Đã xảy ra lỗi khi xóa quyền.");
            }

            return NoContent();
        }


        private bool RoleExists(int id)
        {
            return _context.Quyen.Any(e => e.IDRole == id);
        }
    }
}
