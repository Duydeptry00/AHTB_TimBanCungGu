using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.ViewModels;
using MongoDB.Bson;
using MongoDB.Driver;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class TimBanCungGuController : Controller
    {
        private readonly DBAHTBContext _context;
        private readonly IMongoCollection<BsonDocument> _MatchNguoiDung;
        public TimBanCungGuController(DBAHTBContext context)
        {
            _context = context;
            var connectionString = "mongodb://localhost:27017"; // URI của MongoDB
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("AHTBdb"); // Tên database của bạn
            _MatchNguoiDung = database.GetCollection<BsonDocument>("MatchNguoiDung"); // Tên collection của bạn
        }
          
        // GET: TimBanCungGu
        public async Task<IActionResult> TrangChu()
        {

            var userName = HttpContext.Session.GetString("TempUserName");
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized(new { success = false, message = "Người dùng chưa đăng nhập." });
            }

            var nguoitimdoituong = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (nguoitimdoituong == null)
            {
                return NotFound(new { success = false, message = "Người dùng không tồn tại." });
            }
            var dBAHTBContext = _context.ThongTinCN
          .Include(t => t.User)
          .Include(t => t.AnhCaNhan);


            // Chuyển dữ liệu thành danh sách ViewModel
            var thongTinCaNhanViewModels = await dBAHTBContext.Select(t => new InfoNguoiDung
            {
                IDProfile = t.IDProfile,
                UsID = t.UsID,
                HoTen = t.HoTen,
                Email = t.Email,
                GioiTinh = t.GioiTinh,
                NgaySinh = t.NgaySinh,
                SoDienThoai = t.SoDienThoai,
                IsPremium = t.IsPremium,
                MoTa = t.MoTa,
                NgayTao = t.NgayTao,
                TrangThai = t.TrangThai,
                HinhAnh = t.AnhCaNhan.Select(a => a.HinhAnh).ToList() ?? new List<string>()
            }).Where(x=>x.UsID != nguoitimdoituong.UsID).ToListAsync();

            // Trả về view và truyền dữ liệu qua ViewModel
            return View(thongTinCaNhanViewModels);
        }

   

        // GET: TimBanCungGu/Create
        public IActionResult Create()
        {
            ViewData["UsID"] = new SelectList(_context.Users, "UsID", "UsID");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LuuSwipeAction([FromBody] SwipeRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId2) || string.IsNullOrEmpty(request.Action))
            {
                return BadRequest(new { success = false, message = "Dữ liệu đầu vào không hợp lệ." });
            }

            try
            {
                int iduser2 = int.Parse(request.UserId2);
                var IdDoituong = _context.ThongTinCN.FirstOrDefault(x => x.IDProfile == iduser2 );
                if (IdDoituong == null)
                {
                    return NotFound(new { success = false, message = "Người dùng không tồn tại." });
                }
                var doituong = _context.Users.FirstOrDefault(x=>x.UsID == IdDoituong.UsID);
                var userName = HttpContext.Session.GetString("TempUserName");
                if (string.IsNullOrEmpty(userName))
                {
                    return Unauthorized(new { success = false, message = "Người dùng chưa đăng nhập." });
                }

                var nguoitimdoituong = await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);
                if (nguoitimdoituong == null)
                {
                    return NotFound(new { success = false, message = "Người dùng không tồn tại." });
                }

                var matchNguoiDung = new BsonDocument
                {
                    { "IDMatch", Guid.NewGuid().ToString() },
                    { "User1", nguoitimdoituong.UserName },
                    { "User2", doituong.UserName },
                    { "MatchedAt", DateTime.UtcNow },
                    { "SwipeAction", request.Action }
                };

                await _MatchNguoiDung.InsertOneAsync(matchNguoiDung);

                return Ok(new { success = true, message = "Đã lưu hành động swipe." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        public class SwipeRequest
        {
            public string UserId2 { get; set; }
            public string Action { get; set; }
        }





    }
}
