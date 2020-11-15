using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Online_Auction.Data;
using Online_Auction.Hubs;
using Online_Auction.Models;
using Online_Auction.Services;
using Serilog;

namespace Online_Auction
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

 
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeleteLot, DeleteLot>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();
            services.AddSingleton<ISaveImage, SaveImage>();
            services.AddSignalR();
            services.AddHangfire(i => i.UseSqlServerStorage(
                Configuration.GetConnectionString("DefaultConnection")));
            services.AddControllersWithViews();
        }

 
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //env.EnvironmentName = "Production";
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseSerilogRequestLogging();
           // app.UseStatusCodePagesWithReExecute("/Error/Index", "?statusCode={0}"); 
            app.UseHangfireServer();
            app.UseHangfireDashboard();
            app.UseHttpsRedirection();
            app.UseStaticFiles();  
            
            app.UseRouting();
            
            app.UseAuthentication();  
            app.UseAuthorization(); 
            app.UseSignalR(routes =>
            {
                routes.MapHub<RateHub>($"/Home/ProfileLot");
            });
            app.UseEndpoints(endpoints =>
            { 
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            
        }
    }
}