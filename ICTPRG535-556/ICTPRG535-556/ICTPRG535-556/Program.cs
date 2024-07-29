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
            DataAccess con = new DataAccess();
            con.InitializeDatabase();
            Console.Out.WriteLine(con.GetLists());
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
            services.AddSession(opts =>
            {
                opts.IdleTimeout = TimeSpan.FromMinutes(30); // Increased session timeout to 30 minutes
                opts.Cookie.HttpOnly = true;
                opts.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews();

            // Register other services here, e.g. DataAccess, DbContext, etc.
            // services.AddTransient<DataAccess>(); // If DataAccess is required via DI
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
            app.UseSession(); // Ensure this is before UseAuthorization
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
