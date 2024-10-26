using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            services.AddDistributedMemoryCache();
            // Th�m d?ch v? session
            services.AddSession(options =>
            {
                // Thi?t l?p th?i gian t?n t?i c?a session
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian session s? h?t h?n sau 30 ph�t kh�ng ho?t ??ng
                options.Cookie.HttpOnly = true; // Ch? truy c?p ???c session qua HTTP, b?o m?t h?n b?ng c�ch ng?n JavaScript truy c?p cookie
                options.Cookie.IsEssential = true; // Cookie n�y l� c?n thi?t v� kh�ng b? ?nh h??ng b?i c�c t�y ch?n v? quy?n ri�ng t?
            });
            services.AddHttpClient(); // ??ng k� HttpClient
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            // S? d?ng middleware Session
            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
