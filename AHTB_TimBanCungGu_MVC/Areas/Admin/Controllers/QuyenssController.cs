using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.ViewModels;
using AHTB_TimBanCungGu_MVC.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuyenssController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://localhost:15172/api/Quyens"; // URL API của bạn

        public QuyenssController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Admin/Quyens
        public async Task<IActionResult> Index()
        {
            // Lấy token JWT và UserType từ session
            var token = HttpContext.Session.GetString("JwtToken");
            var userType = HttpContext.Session.GetString("UserType");

            if (userType == "Admin" && token != null)
            {
                var roles = await _httpClient.GetFromJsonAsync<IEnumerable<RoleVM>>(_apiUrl);
                return View(roles); // Trả về danh sách RoleVM để hiển thị thông tin quyền
            }

            return NotFound();
          
        }

        // GET: Admin/Quyens/Create
        public IActionResult Create()
        {
            var model = new RoleListVM(); // Khởi tạo danh sách quyền
            return PartialView("_CreateRole", model); // Trả về PartialView với danh sách quyền rỗng
        }

        // POST: Admin/Quyens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleListVM roleListVM)
        {
            if (roleListVM == null || roleListVM.Roles == null || !roleListVM.Roles.Any())
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu quyền bị rỗng.");
                return PartialView("Index", roleListVM);
            }

            // Lọc các quyền có Module không rỗng
            var validRoles = roleListVM.Roles.Where(role => !string.IsNullOrEmpty(role.Module)).ToList();

            // Gộp dữ liệu cho tất cả các quyền (cả quyền có Module rỗng và có Module không rỗng)
            var payload = new
            {
                TenRole = roleListVM.TenRole,
                Module = string.Join(", ", roleListVM.Roles.Select(role => role.Module)), // Gộp tất cả Module (bao gồm cả Module rỗng)
                Add = string.Join(", ", roleListVM.Roles.Select(role => role.Add)),       // Gộp tất cả Add
                Update = string.Join(", ", roleListVM.Roles.Select(role => role.Update)), // Gộp tất cả Update
                Delete = string.Join(", ", roleListVM.Roles.Select(role => role.Delete)), // Gộp tất cả Delete
                ReviewDetails = string.Join(", ", roleListVM.Roles.Select(role => role.ReviewDetails)) // Gộp tất cả ReviewDetails
            };

            // Gửi dữ liệu gộp đến API
            var response = await _httpClient.PostAsJsonAsync(_apiUrl, payload);

            // Kiểm tra phản hồi từ API
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi tạo quyền mới.");
                return PartialView("Index", roleListVM); // Trả lại PartialView nếu có lỗi
            }

            // Nếu thành công, chuyển hướng đến trang Index
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = await _httpClient.GetFromJsonAsync<RoleVM>($"{_apiUrl}/{id}");
            if (role == null)
            {
                return NotFound();
            }

            // Chuyển đổi dữ liệu cho RoleVMUpdate
            var roleUpdate = new RoleVMUpdate
            {
                IDRole = role.IDRole,
                TenRole = role.TenRole,
                Module = role.Module?.Split(", ")?.ToList(),
                Add = role.Add?.Split(", ")?.ToList(),
                Update = role.Update?.Split(", ")?.ToList(),
                Delete = role.Delete?.Split(", ")?.ToList(),
                ReviewDetails = role.ReviewDetails?.Split(", ")?.ToList()
            };

            // Trả về PartialView với dữ liệu đã chuyển đổi
            return PartialView("_UpdateRole", roleUpdate);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleVMUpdate roleVM)
        {
            // Filter roles where the Module is not empty
            var validRoles = roleVM.Module
                .Where(module => !string.IsNullOrEmpty(module))
                .ToList();
            // Combine the values in the lists into comma-separated strings
            var payload = new
            {
                TenRole = roleVM.TenRole,
                Module = string.Join(", ", roleVM.Module), // Join the list of modules into a single comma-separated string
                Add = string.Join(", ", roleVM.Add),        // Join the list of "Add" actions into a single comma-separated string
                Update = string.Join(", ", roleVM.Update),  // Join the list of "Update" actions
                Delete = string.Join(", ", roleVM.Delete),  // Join the list of "Delete" actions
                ReviewDetails = string.Join(", ", roleVM.ReviewDetails) // Join the list of "ReviewDetails" actions
            };

            if (ModelState.IsValid)
            {
                // Send the payload to your API
                var response = await _httpClient.PutAsJsonAsync($"{_apiUrl}/{id}", payload);

                if (response.IsSuccessStatusCode)
                {
                    // Redirect to the Index action if the update is successful
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Add an error to the model state if the request fails
                    ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật quyền.");
                }
            }
            return RedirectToAction(nameof(Index));
        }


        // POST: Admin/Quyens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Kiểm tra xem quyền có tồn tại không trước khi xóa
            var role = await _httpClient.GetFromJsonAsync<RoleVM>($"{_apiUrl}/{id}");
            if (role == null)
            {
                return NotFound("Không tìm thấy quyền cần xóa.");
            }

            // Gửi yêu cầu xóa đến API
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đã xóa quyền thành công."; // Thông báo thành công
                return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách quyền
            }

            // Nếu có lỗi khi xóa, thêm thông báo lỗi
            TempData["ErrorMessage"] = "Lỗi khi xóa quyền.";
            return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách quyền
        }

    }
}
