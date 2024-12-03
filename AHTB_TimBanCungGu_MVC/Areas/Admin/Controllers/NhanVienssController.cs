using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.ViewModels;
using AHTB_TimBanCungGu_MVC.Models;
using DocumentFormat.OpenXml.Spreadsheet;
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
using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhanVienssController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string _ApiBaseUrl = "http://localhost:15172/api";
        private const string ApiBaseUrl = "http://localhost:15172/api/NhanViens";
        private readonly string _apiUrl = "http://localhost:15172/api/PhanQuyens";
        private readonly string _RoleapiUrl = "http://localhost:15172/api/Quyens";// API để quản lý User_Role
        public NhanVienssController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string usernameFilter = "", string roleFilter = "")
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                var apiUrl = $"{ApiBaseUrl}";
                var url = apiUrl;

                // Nếu có bộ lọc username, thêm vào URL
                if (!string.IsNullOrEmpty(usernameFilter))
                {
                    url = apiUrl + "/UserName";
                    url += $"?query={usernameFilter}";
                }

                // Nếu có bộ lọc role, thêm vào URL
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    url = apiUrl + "/UserName";
                    url += (url.Contains("?") ? "&" : "?") + $"roleFilter={roleFilter}";
                }

                // Gửi yêu cầu GET tới API
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    // Đọc nội dung JSON từ response
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var nhanViens = JsonConvert.DeserializeObject<List<NhanVienVM>>(jsonData) ?? new List<NhanVienVM>();

                    // Phân trang kết quả
                    var totalItems = nhanViens.Count;
                    var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                    var paginatedList = nhanViens.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                    // Tạo ViewModel và trả về View
                    var viewModel = new NhanVienListViewModel
                    {
                        NhanViens = paginatedList,
                        PageNumber = page,
                        TotalPages = totalPages,
                        UsernameFilter = usernameFilter,
                        RoleFilter = roleFilter,
                        PageSize = pageSize // Truyền pageSize tới view
                    };

                    return View(viewModel);
                }

                // Nếu API không trả về kết quả thành công, trả về ViewModel trống
                return View(new NhanVienListViewModel());
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
            if (!ModelState.IsValid)
            {
                return Json(new { Success = false, Message = "Dữ liệu không hợp lệ." });
            }
            if (string.IsNullOrEmpty(nhanVien.UserName))
            {
                return Json(new { Success = false, Message = "UserName không được để trống!" });
            }
            if (string.IsNullOrEmpty(nhanVien.Password))
            {
                return Json(new { Success = false, Message = "Password không được để trống!" });
            }
            if (string.IsNullOrEmpty(nhanVien.Email) || !IsValidEmail(nhanVien.Email))
            {
                return Json(new { Success = false, Message = "Địa chỉ email không đúng định dạng!" });
            }
            try
            {
                // Ensure username is in lowercase
                nhanVien.UserName = nhanVien.UserName.ToLower();

                // Serialize the user object
                var jsonContent = JsonConvert.SerializeObject(nhanVien);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send the user data to the API
                var response = await _httpClient.PostAsync($"{ApiBaseUrl}", content);

                if (response.IsSuccessStatusCode)
                {
                    // Prepare data for sending an invitation
                    string local = GlobalSettings.MvcBaseUrl;
                    string email = nhanVien.Email;
                    var inviteData = new { email, local };
                    var content2 = new StringContent(
                        JsonConvert.SerializeObject(inviteData),
                        Encoding.UTF8,
                        "application/json");

                    // Send the invitation
                    var inviteResponse = await _httpClient.PostAsync($"{_ApiBaseUrl}/Account/SendEmployeeInvite", content2);

                    if (inviteResponse.IsSuccessStatusCode)
                    {
                        return Json(new { Success = true, Message = "Thêm nhân viên và gửi thư mời thành công." });
                    }

                    return Json(new { Success = false, Message = "Nhân viên đã được thêm, nhưng không thể gửi thư mời." });
                }

                return Json(new { Success = false, Message = "Tài khoản hoặc email đã tồn tại." });
            }
            catch (Exception ex)
            {
                // Log exception (use your logging framework)
                Console.WriteLine(ex.Message);

                return Json(new { Success = false, Message = "Đã xảy ra lỗi trong quá trình xử lý." });
            }
        }


        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
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
             id = nhanVien.IdNhanVien;
            if (ModelState.IsValid)
            {
                var jsonContent = JsonConvert.SerializeObject(nhanVien);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{ApiBaseUrl}/{id}", content);
                if (response.IsSuccessStatusCode)
                {
                    return Json(new { Success = true, Message = "Sửa nhân viên thành công!" });
                }
                return Json(new { Success = false, Message = "Có lỗi xảy ra khi sửa nhân viên." });
            }
            return Json(new { Success = false, Message = "Dữ liệu không hợp lệ." });
        }

        // POST: Admin/NhanVienss/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                return Ok(); // Trả về OK nếu xóa thành công
            }
            return Problem("Có lỗi xảy ra khi xóa nhân viên.");
        }
        public async Task<IActionResult> Authorize(int page = 1, int pageSize = 5)
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                try
                {
                    // Thêm các tham số phân trang vào URL API
                    var queryString = $"?page={page}&pageSize={pageSize}";

                    // Lấy danh sách người dùng từ API
                    var danhSachNguoiDung = await _httpClient.GetFromJsonAsync<List<NhanVienVM>>(ApiBaseUrl);
                    var danhSachNguoiDungGomUsername = danhSachNguoiDung
                        .Where(u => u.TrangThai == "Đang Làm Việc")  // Lọc người dùng có trạng thái "Đang Làm Việc"
                        .Select(u => u.UserName)  // Chỉ lấy tên người dùng
                        .ToList();

                    // Lấy danh sách phân quyền từ API với phân trang
                    var responsePhanQuyen = await _httpClient.GetFromJsonAsync<List<ListPhanQuyen>>(_apiUrl);

                    if (responsePhanQuyen == null)
                    {
                        return View("Error", new { message = "Không thể tải danh sách phân quyền" });
                    }

                    // Lấy danh sách vai trò từ API
                    var roles = await _httpClient.GetFromJsonAsync<List<RoleVM>>(_RoleapiUrl);
                    if (roles == null)
                    {
                        return View("Error", new { message = "Không thể tải danh sách vai trò" });
                    }

                    var totalPhanQuyen = responsePhanQuyen.Count; // Tổng số phân quyền
                    var totalPages = (int)Math.Ceiling((double)totalPhanQuyen / pageSize); // Tính tổng số trang

                    // Phân trang dữ liệu phân quyền (Chỉ lấy dữ liệu cho trang hiện tại)
                    var phanQuyen = responsePhanQuyen.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                    // Tạo model và trả về view
                    var model = new ListUser_role
                    {
                        PhanQuyen = phanQuyen,
                        Users = danhSachNguoiDungGomUsername.Select(u => new NhanVienVM { UserName = u }).ToList(),
                        Roles = roles,
                        RolesList = new List<User_role>(),
                        PageNumber = page,
                        TotalPages = totalPages,
                        PageSize = pageSize
                    };

                    return View(model);
                }
                catch (Exception ex)
                {
                    return View("Error", new { message = "Đã có lỗi xảy ra: " + ex.Message });
                }
            }

            return NotFound();
           
        }

        // POST: Admin/NhanVienss/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteUserRole")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                // Return to the Index page after successful deletion
                TempData["SuccessMessage"] = "Xóa quyền người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa quyền người dùng.";
                return RedirectToAction(nameof(Index)); // Redirect back to the Index page
            }
        }

        // POST: Admin/Authorize
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authorize(ListUser_role userRole)
        {
            // Kiểm tra nếu danh sách người dùng (User) là null, khởi tạo danh sách rỗng
            if (userRole.User == null)
            {
                userRole.User = new List<string>(); // Sử dụng List<string> nếu bạn chỉ cần lưu trữ tên người dùng (username)
            }

            // Kiểm tra tính hợp lệ của model
            if (!ModelState.IsValid)
            {
                return View(userRole); // Trả lại form nếu model không hợp lệ
            }

            // Danh sách để lưu tên người dùng đã có quyền
            var usersWithRole = new List<string>();
            var usersSuccess = new List<string>();
            // Cập nhật RolesList cho từng User (nếu chưa có)
            foreach (var username in userRole.User)
            {
                // Gọi API để kiểm tra quyền
                var checkRoleResponse = await _httpClient.GetAsync($"{ApiBaseUrl}/CheckRole?UserName={username}");

                if (!checkRoleResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, $"Không thể kiểm tra quyền cho người dùng: {username}");
                    return View(userRole);
                }

                // Kiểm tra kết quả từ API
                var roleExists = await checkRoleResponse.Content.ReadFromJsonAsync<bool>();
                if (roleExists)
                {
                    // Thêm người dùng đã có quyền vào danh sách
                    usersWithRole.Add(username);
                    continue; // Bỏ qua việc cấp quyền cho người dùng này
                }

                // Tạo đối tượng dữ liệu để gửi lên API
                var userRoleRequest = new User_role
                {
                    Username = username, // Dùng Username từ danh sách User
                    Id_Role = userRole.Id_Role, // Dùng Id_Role từ vai trò
                };

                // Gửi yêu cầu tới API để gán quyền
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, userRoleRequest);
                if (response.IsSuccessStatusCode)
                {
                    // Nếu API trả về thành công, thêm người dùng vào danh sách đã cấp quyền
                    usersSuccess.Add(username);
                }
                else
                {
                    // Nếu có lỗi, thêm lỗi vào ModelState và trả về lại view
                    ModelState.AddModelError(string.Empty, $"Lỗi khi tạo quyền mới cho người dùng: {username}");
                    return View(userRole);
                }
            }

            // Truyền thông báo về danh sách người dùng đã có quyền
            if (usersWithRole.Count > 0)
            {
                var usersMessage = $" {string.Join(", ", usersWithRole)} đã cấp quyền sẵn!";
                var usersSuccessMessage = "";
                if ((usersSuccess.Count > 0))
                {
                    usersSuccessMessage = $" {string.Join(", ", usersSuccess)} đã cấp quyền thành công!";
                }
               
                // Trả thông báo JSON với message thay đổi
                return Json(new
                {
                    success = true,
                    message = usersMessage, // Gửi thông báo với danh sách người dùng đã có quyền
                    usersWithRole = usersWithRole,
                    roleId = userRole.Id_Role,
                    usersSuccess = usersSuccessMessage
                });
            }
            else if (usersSuccess.Count > 0)
            {
                var usersSuccessMessage = $" {string.Join(", ", usersSuccess)} đã cấp quyền thành công!";
                return Json(new
                {
                    success = false,
                    message = "Không có người dùng nào có quyền hiện tại.",
                    usersWithRole = usersWithRole,
                    roleId = userRole.Id_Role,
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi! Không có người dùng nào được quyền hiện tại.",
                });
            }
        }
        // POST: Admin/UpdateRole
        [HttpPost]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            var usersSuccess = new List<string>();

            // Cập nhật RolesList cho từng User (nếu chưa có)
            foreach (var username in request.UsersWithRole)
            {
                // Tạo đối tượng dữ liệu để gửi lên API
                var userRoleRequest = new User_role
                {
                    Username = username, // Dùng Username từ danh sách User
                    Id_Role = request.IdRole, // Dùng Id_Role từ vai trò
                };

                // Gửi yêu cầu POST tới API để gán quyền cho người dùng
                var response = await _httpClient.PutAsJsonAsync($"{_apiUrl}/UpdateRole", userRoleRequest);
                if (response.IsSuccessStatusCode)
                {
                    // Nếu API trả về thành công, thêm người dùng vào danh sách đã cấp quyền
                    usersSuccess.Add(username);
                }
                else
                {
                    // Nếu có lỗi, thêm lỗi vào ModelState và trả về lại view
                    ModelState.AddModelError(string.Empty, $"Lỗi khi tạo quyền mới cho người dùng: {username}");
                    return View();
                }
            }
            if (usersSuccess.Count > 0)
            {
                var usersSuccessMessage = $" {string.Join(", ", usersSuccess)} đã cấp quyền thành công!";
                return Json(new
                {
                    success = true,
                    usersSuccessMessage = usersSuccessMessage,
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi! Không có người dùng nào được quyền hiện tại.",
                });
            }
        }
        public class UpdateRoleRequest
        {
            public List<string> UsersWithRole { get; set; }
            public int IdRole { get; set; }
        }

    }
}
