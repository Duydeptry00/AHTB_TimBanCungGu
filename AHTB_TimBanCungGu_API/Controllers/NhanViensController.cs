using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhanViensController : ControllerBase
    {
        private readonly DBAHTBContext _context;

        public NhanViensController(DBAHTBContext context)
        {
            _context = context;
        }

        // GET: api/NhanViens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhanVienVM>>> GetAllNhanViens()
        {
            // Fetch users and their associated personal information
            var users = await _context.Users
                .Include(u => u.ThongTinCN) // Tải thông tin cá nhân liên kết
                .Include(u => u.User_Role)  // Giả sử User_Role là bảng trung gian kết nối với Role
                .ThenInclude(ur => ur.Role) // Nạp dữ liệu từ bảng Role qua quan hệ trung gian
                .ToListAsync();

            // Map to NhanVienVM
            var userViewModels = users.Select((user, index) => new NhanVienVM
            {
                STT = index + 1,  // Số thứ tự, bắt đầu từ 1
                IdNhanVien = user.UsID,
                UserName = user.UserName,
                Email = user.ThongTinCN?.Email, // Kiểm tra null cho an toàn
                TrangThai = user.TrangThai,
                Quyen = user.User_Role != null && user.User_Role.Any()
                    ? string.Join(", ", user.User_Role.Select(ur => ur.Role.Module)) // Lấy tên quyền từ Role
                    : "Chưa cấp quyền cho nhân viên" // Hiển thị thông báo nếu không có quyền
            }).ToList();

            return Ok(userViewModels); // Trả về danh sách view model đã ánh xạ
        }

        // GET: api/NhanViens/UserName?query=nha
        [HttpGet("UserName")]
        public async Task<ActionResult<IEnumerable<NhanVienVM>>> GetNhanViensByUserName(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query parameter is required");
            }

            var users = await _context.Users
                .Include(u => u.ThongTinCN)
                 .Include(u => u.User_Role)
                 .ThenInclude(ur => ur.Role)
                                       .Where(u => u.UserName.Contains(query))
                                       .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound();
            }

            // Map to NhanVienVM
            var userViewModels = users.Select((user, index) => new NhanVienVM
            {
                STT = index + 1,  // Số thứ tự, bắt đầu từ 1
                IdNhanVien = user.UsID,
                UserName = user.UserName,
                Email = user.ThongTinCN.Email, // Kiểm tra null cho an toàn
                TrangThai = user.TrangThai,
                Quyen = user.User_Role != null && user.User_Role.Any()
                    ? string.Join(", ", user.User_Role.Select(ur => ur.Role.Module)) // Lấy tên quyền từ Role
                    : "Chưa cấp quyền cho nhân viên" // Hiển thị thông báo nếu không có quyền
            }).ToList();

            return userViewModels;
        }




        // GET: api/NhanViens/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetNhanVien(string id)
        {
            var nhanVien = await _context.Users.FindAsync(id);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return nhanVien;
        }

        // POST: api/NhanViens
        [HttpPost]
        public async Task<ActionResult<NhanVienVM>> AddNhanVien(NhanVienVM nhanVien)
        {
            // Kiểm tra tính hợp lệ của mô hình
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Mã hóa mật khẩu nếu nó không rỗng
            if (!string.IsNullOrEmpty(nhanVien.Password))
            {
                nhanVien.Password = BCrypt.Net.BCrypt.HashPassword(nhanVien.Password);
            }

            // Tạo một mã ID mới cho nhân viên
            var userId = Guid.NewGuid().ToString();

            // Tạo đối tượng User mới
            var newUser = new User
            {
                UsID = userId,
                UserName = nhanVien.UserName,
                Password = nhanVien.Password,
                TrangThai = "Hoạt Động" // Trạng thái
            };

            // Thêm người dùng vào cơ sở dữ liệu
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Tạo thông tin cá nhân mới
            var thongTinCaNhan = new ThongTinCaNhan
            {
                UsID = userId, // Liên kết với ID của người dùng
                Email = nhanVien.Email, // Email từ NhanVienVM
                HoTen = "", // Giá trị mặc định hoặc giá trị từ nơi khác
                GioiTinh = "", // Giá trị mặc định
                NgaySinh = DateTime.Now, // Hoặc ngày mặc định
                DiaChi = "",
                SoDienThoai = "",
                IsPremium = false,
                MoTa = "",
                NgayTao = DateTime.Now,
                TrangThai = "Hoạt Động"
            };

            // Thêm thông tin cá nhân vào DbContext
            _context.ThongTinCN.Add(thongTinCaNhan);

            // Lưu các thay đổi
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetNhanVien", new { id = userId }, nhanVien);
        }



        // PUT: api/NhanViens/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNhanVien(string id, NhanVienVM nhanVien)
        {
            if (id != nhanVien.IdNhanVien)
            {
                return BadRequest("ID không khớp với nhân viên.");
            }

            // Lấy thông tin hiện tại của nhân viên
            var existingNhanVien = await _context.Users.FindAsync(id);
            if (existingNhanVien == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            // Cập nhật các trường không phải mật khẩu
            existingNhanVien.UserName = nhanVien.UserName;
            existingNhanVien.TrangThai = nhanVien.TrangThai;
            // Nếu bạn muốn cập nhật thêm các trường khác, bạn có thể thêm vào đây

            // Nếu mật khẩu được cung cấp, mã hóa và cập nhật
            if (!string.IsNullOrEmpty(nhanVien.Password))
            {
                existingNhanVien.Password = BCrypt.Net.BCrypt.HashPassword(nhanVien.Password);
            }

            // Lưu thay đổi
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NhanVienExists(id))
                {
                    return NotFound("Không tìm thấy nhân viên.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/NhanViens/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNhanVien(string id)
        {
            // Tìm nhân viên
            var nhanVien = await _context.Users.FindAsync(id);
            if (nhanVien == null)
            {
                return NotFound("Không tìm thấy nhân viên để xóa.");
            }

            // Tìm thông tin cá nhân dựa trên UsID của nhân viên
            var thongTinCaNhan = await _context.ThongTinCN.FirstOrDefaultAsync(t => t.UsID == id);

            using var transaction = await _context.Database.BeginTransactionAsync(); // Sử dụng giao dịch để đảm bảo tính nhất quán

            try
            {
                if (thongTinCaNhan != null)
                {
                    // Xóa thông tin cá nhân
                    _context.ThongTinCN.Remove(thongTinCaNhan);
                }

                // Xóa nhân viên
                _context.Users.Remove(nhanVien);

                // Lưu thay đổi
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent(); // Xóa thành công
            }
            catch (Exception ex)
            {
                // Nếu xảy ra lỗi, cập nhật trạng thái nhân viên thành "Đình Chỉ"
                await transaction.RollbackAsync();
                nhanVien.TrangThai = "Đình Chỉ";

                try
                {
                    // Lưu thay đổi trạng thái vào cơ sở dữ liệu
                    await _context.SaveChangesAsync();
                }
                catch (Exception innerEx)
                {
                    // Xử lý lỗi khi không thể cập nhật trạng thái (nếu cần)
                    return StatusCode(500, $"Lỗi trong khi cập nhật trạng thái: {innerEx.Message}");
                }

                return StatusCode(500, $"Lỗi khi xóa nhân viên: {ex.Message}");
            }
        }



        private bool NhanVienExists(string id)
        {
            return _context.Users.Any(e => e.UsID == id);
        }
    }
}
