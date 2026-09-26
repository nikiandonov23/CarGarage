using CarGarage.Data;
using CarGarage.Services.Core;
using CarGarage.Services.Core.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // сървисите дето ше си добавям
            builder.Services.AddScoped<IMyCarsService, MyCarsService>();
            builder.Services.AddScoped<ICloudflareR2Service, CloudflareR2Service>();
            builder.Services.AddScoped<ISearchService, SearchService>();
            builder.Services.AddScoped<IPartsService, PartsService>();
            builder.Services.AddScoped<IMarketplaceService, MarketplaceService>();
            builder.Services.AddScoped<IOffersService, OffersService>();
            // Ensure offers and messages services are registered
            builder.Services.AddScoped<IMessagesService, MessagesService>();
            builder.Services.AddScoped<IInvoicesService, InvoicesService>();
            builder.Services.AddScoped<ICustomersService, CustomersService>();
            builder.Services.AddScoped<IGarageService, GarageService>();



            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();
            // SignalR for real-time notifications
            builder.Services.AddSignalR();

            var app = builder.Build();

        

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // SignalR hubs
            app.MapHub<CarGarage.Notifications.NotificationsHub>("/notificationsHub");

            app.MapRazorPages();

            app.Run();
        }
    }
}