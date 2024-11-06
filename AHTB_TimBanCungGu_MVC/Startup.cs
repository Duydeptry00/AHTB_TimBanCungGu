using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.ViewModels;
using AHTB_TimBanCungGu_MVC.Local;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IVnPayService, VnPayService>();
            services.AddDistributedMemoryCache();
            // Thêm d?ch v? session
            services.AddSession(options =>
            {
                // Thi?t l?p th?i gian t?n t?i c?a session
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian session s? h?t h?n sau 30 phút không ho?t ??ng
                options.Cookie.HttpOnly = true; // Ch? truy c?p ???c session qua HTTP, b?o m?t h?n b?ng cách ng?n JavaScript truy c?p cookie
                options.Cookie.IsEssential = true; // Cookie này là c?n thi?t và không b? ?nh h??ng b?i các tùy ch?n v? quy?n riêng t?
            });
            services.AddHttpClient(); // ??ng ký HttpClient
            services.AddControllersWithViews();
            services.AddDbContext<DBAHTBContext>(options =>
        options.UseSqlServer(Configuration.GetConnectionString("DBConnection")));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AHTB_TimBanCungGu_API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseWebSockets();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            // Đăng ký middleware MvcBaseUrlMiddleware
            app.UseMiddleware<MvcBaseUrlMiddleware>();

            app.UseRouting();

            app.UseAuthorization();

            // S? d?ng middleware Session
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {

                // Route cho Area, cho phép truy cập bằng cách sử dụng tham số area
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller}/{action}/{id?}"); // Sử dụng pattern cho Area

                // Route mặc định
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
