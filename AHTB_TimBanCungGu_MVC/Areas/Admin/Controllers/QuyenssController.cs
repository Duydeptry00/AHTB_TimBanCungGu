using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.ViewModels;
using AHTB_TimBanCungGu_MVC.Models;

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
            var roles = await _httpClient.GetFromJsonAsync<IEnumerable<RoleVM>>(_apiUrl);
            return View(roles); // Trả về danh sách RoleVM để hiển thị thông tin quyền
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

                foreach (var roleVM in roleListVM.Roles)
                {
                    // Gửi từng roleVM đến API
                    var response = await _httpClient.PostAsJsonAsync(_apiUrl, roleVM);
                    if (!response.IsSuccessStatusCode)
                    {
                        ModelState.AddModelError(string.Empty, "Lỗi khi tạo quyền mới cho module " + roleVM);
                        return PartialView("_CreateRole", roleListVM); // Trả lại PartialView nếu có lỗi
                    }
                }

                return RedirectToAction(nameof(Index)); // Nếu tất cả thành công, chuyển đến trang Index // Trả lại PartialView nếu ModelState không hợp lệ
        }


        // GET: Admin/Quyens/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _httpClient.GetFromJsonAsync<RoleVM>($"{_apiUrl}/{id}"); // Change to RoleVM if your API returns it
            if (role == null)
            {
                return NotFound();
            }
            return PartialView("_UpdateRole", role); // Return partial view for Update
        }

        // POST: Admin/Quyens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleVM roleVM)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpClient.PutAsJsonAsync($"{_apiUrl}/{id}", roleVM);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật quyền.");
            }
            return PartialView("_UpdateRole", roleVM); // Return partial view if invalid
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
