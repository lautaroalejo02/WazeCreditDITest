using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WazeCredit.Data;
using WazeCredit.Middleware;
using WazeCredit.Service;
using WazeCredit.Service.LifeTimeExample;
using WazeCredit.Utility.AppSettingsClasses;
using WazeCredit.Utility.DI_Config;

namespace WazeCredit
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            //We need to register our services before we can use them.
            services.AddTransient<IMarketForecaster,MarketForecasterV2>();
            services.AddScoped<IValidationChecker, AddressValidationChecker>();
            services.AddScoped<IValidationChecker, CreditValidationChecker>();
            services.AddScoped<ICreditValidator, CreditValidator>();
            services.AddTransient<TransientService>();
            services.TryAddScoped<ScopedService>();//try add checks if its not registered and if its not, then it registers it.
            services.AddSingleton<SingletonService>();
            //Replace is used to replace 
            //services.Replace(ServiceDescriptor.Transient<IMarketForecaster, MarketForecaster>());
            //services.AddSingleton<IMarketForecaster>(new MarketForecaster()); //Can only be singleton
            //services.AddTransient<IMarketForecaster>();
            //implementation
            //services.AddSingleton(typeof(MarketForecasterV2)); //This can only be singleton
            //abstracion and implementation
            //services.AddTransient(typeof(IMarketForecaster), typeof(MarketForecasterV2));
            //we need to add addrazorruntimecompilation which has been previously installed via nuget
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddRazorPages();
            //aca llamo a todos los servicios en vez de tenerlos todos aca
            services.AddAppSettingsConfig(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();
            //We need to add this in order to be able to use the middleware class that we had created previously.
            app.UseMiddleware<CustomMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
