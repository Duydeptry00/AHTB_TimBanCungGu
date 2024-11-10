using AHTB_TimBanCungGu_API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_API
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
            // C?u hình xác th?c JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, // Ki?m tra Issuer
                        ValidateAudience = true, // Ki?m tra Audience
                        ValidateLifetime = true, // Ki?m tra th?i gian h?t h?n
                        ValidIssuer = "Admin", // Issuer (có th? thay b?ng giá tr? c?a b?n)
                        ValidAudience = "Admin", // Audience (có th? thay b?ng giá tr? c?a b?n)
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("AHTB_DATN")) // SecretKey c?a b?n
                    };
                });
            services.AddDbContext<DBAHTBContext>(options =>
         options.UseSqlServer(Configuration.GetConnectionString("DBConnection")));
            services.AddMemoryCache();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AHTB_TimBanCungGu_API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AHTB_TimBanCungGu_API v1"));
            }
            app.UseWebSockets();

            app.UseRouting();
            // C?u hình middleware JWT
            app.UseAuthentication(); // Ph?i g?i tr??c UseAuthorization
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
