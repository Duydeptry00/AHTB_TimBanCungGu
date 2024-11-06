using AHTB_TimBanCungGu_API.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_MVC.Local
{
    public class MvcBaseUrlMiddleware
    {

        private readonly RequestDelegate _next;

        public MvcBaseUrlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Lưu địa chỉ URL của ứng dụng MVC vào biến toàn cục
            GlobalSettings.MvcBaseUrl = $"{context.Request.Scheme}://{context.Request.Host}";

            // Gọi middleware tiếp theo trong pipeline
            await _next(context);
        }
    }
}
