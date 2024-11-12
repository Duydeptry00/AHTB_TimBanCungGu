using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.ViewModels;
using AHTB_TimBanCungGu_MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhanVienssController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:15172/api/NhanViens";
        private readonly string _apiUrl = "http://localhost:15172/api/PhanQuyens";
        private readonly string _RoleapiUrl = "http://localhost:15172/api/Quyens";// API để quản lý User_Role
        public NhanVienssController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Admin/NhanVienss
        public async Task<IActionResult> Index()
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");
            // Lấy JWT UserType từ Session
            var UserType = HttpContext.Session.GetString("UserType");
            if(UserType == "Admin" && token != null)
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var nhanViens = JsonConvert.DeserializeObject<List<NhanVienVM>>(jsonData);
                    return View(nhanViens);
                }
                return View(new List<NhanVienVM>());
            }
            return NotFound();
        }

        // GET: Admin/NhanVienss/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var nhanVien = JsonConvert.DeserializeObject<AHTB_TimBanCungGu_API.Models.User>(jsonData);
                return View(nhanVien);
            }
            return NotFound();
        }

        // GET: Admin/NhanVienss/Create
        public IActionResult Create()
        {
            return PartialView("_CreateUser", new NhanVienVM()); // Trả về Partial View với đối tượng User mới
        }

        // POST: Admin/NhanVienss/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVienVM nhanVien)
        {
            if (ModelState.IsValid)
            {
                var jsonContent = JsonConvert.SerializeObject(nhanVien);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{ApiBaseUrl}", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                    return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách người dùng
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm nhân viên.";
            }

            // Nếu không hợp lệ, trả lại Partial View với thông tin đã nhập
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(string id)
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var nhanVien = JsonConvert.DeserializeObject<NhanVienVM>(jsonData);
                return PartialView("_EditUserPartial", nhanVien); // Trả về partial view
            }
            return NotFound();
        }

        // POST: Admin/NhanVienss/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, NhanVienVM nhanVien)
        {
            nhanVien.IdNhanVien = id;
            if (ModelState.IsValid)
            {
                var jsonContent = JsonConvert.SerializeObject(nhanVien);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{ApiBaseUrl}/{id}", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Sửa nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi sửa nhân viên.";
            }
            return View(nhanVien);
        }

        // POST: Admin/NhanVienss/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(NhanVienVM id)
        {
            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/{id.IdNhanVien}");
            if (response.IsSuccessStatusCode)
            {
                return Ok(); // Trả về OK nếu xóa thành công
            }
            return Problem("Có lỗi xảy ra khi xóa nhân viên.");
        }
        public async Task<IActionResult> Authorize()
        {
            // Lấy danh sách người dùng từ API
            var danhSachNguoiDung = await _httpClient.GetFromJsonAsync<List<NhanVienVM>>(ApiBaseUrl);
            var danhSachNguoiDungGomUsername = danhSachNguoiDung.Select(u => u.UserName).ToList();

            var model = new ListUser_role
            {
                Users = danhSachNguoiDungGomUsername.Select(u => new NhanVienVM
                {
                    UserName = u
                }).ToList() ?? new List<NhanVienVM>(), // Khởi tạo danh sách nếu rỗng
                Roles = await _httpClient.GetFromJsonAsync<List<RoleVM>>(_RoleapiUrl),
                RolesList = new List<User_role>()
            };

            return View(model);
        }

        // POST: Admin/Authorize
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authorize(ListUser_role userRole)
        {
            if (userRole.User == null)
            {
                userRole.User = new List<NhanVienVM>(); // Khởi tạo danh sách user nếu null
            }

            if (!ModelState.IsValid)
            {
                return View(userRole); // Trả lại form nếu model không hợp lệ
            }

            // Cập nhật RolesList cho từng User (nếu chưa có)
            foreach (var user in userRole.User)
            {
                // Tạo đối tượng dữ liệu để gửi lên API
                var userRoleRequest = new User_role
                {
                    Tenrole = userRole.Tenrole,  // Tên vai trò
                    Username = user.UserName,  // Dùng UserName của nhân viên
                    Id_Role = userRole.Role,    // Dùng Id_Role của vai trò
                };

                // Gửi yêu cầu tới API để gán quyền
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, userRoleRequest);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, $"Lỗi khi tạo quyền mới cho người dùng: {user.UserName}");
                    return View(userRole); // Trả lại form nếu có lỗi
                }
            }
            return RedirectToAction(); // Chuyển đến trang Index khi thành công
        }
    }
}
