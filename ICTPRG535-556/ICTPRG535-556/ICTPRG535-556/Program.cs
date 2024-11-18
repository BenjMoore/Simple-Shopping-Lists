using DataMapper;
using ICTPRG535_556.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace ICTPRG535_556
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public void ConfigureServices(IServiceCollection services)
        {
            // Call SetupDatabaseAndTables when the application starts
            var dataAccess = new DataAccess();
            dataAccess.SetupDatabaseAndTables(); // Only once, ideally in a migration or initialization service

            services.AddSession(opts =>
            {
                opts.IdleTimeout = TimeSpan.FromMinutes(30);
                opts.Cookie.HttpOnly = true;
                opts.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "produceSearch",
                    pattern: "Produce/Search",
                    defaults: new { controller = "Produce", action = "Search" });
            });
        }
    }
}
