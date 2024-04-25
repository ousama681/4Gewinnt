using VierGewinnt.Models;
using VierGewinnt.Repositories.Interfaces;
using VierGewinnt.Repositories;
using VierGewinnt.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using VierGewinnt.ViewModels.GameLobby;
using System.Net;

namespace VierGewinnt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"
            // "Server=Koneko\\KONEKO;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"
            builder.Services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer("Server=Koneko\\KONEKO;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"));

            //builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<AppDbContext>()
            //    .AddDefaultTokenProviders();

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            //.AddUserManager<UserManager<ApplicationUser>>()
            //.AddSignInManager<SignInManager<ApplicationUser>>();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                options.SignIn.RequireConfirmedEmail = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20);
                options.Lockout.MaxFailedAccessAttempts = 10;
            });

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(5);
            });

            builder.Services.ConfigureApplicationCookie(config =>
            {
                IConfiguration _conf = builder.Configuration;
                config.LoginPath = _conf["Application:LoginPath"];
            });

            builder.Services.Configure<SMTPConfigModel>(builder.Configuration.GetSection("SMTPConfig"));

            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseStaticFiles(); // For 3D Homepage,  Set up custom content types - associating file extension to MIME type
            FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();

            // The MIME type for .GLB and .GLTF files are registered with IANA under the 'model' heading
            // https://www.iana.org/assignments/media-types/media-types.xhtml#model
            provider.Mappings[".glb"] = "model/gltf+binary";
            provider.Mappings[".gltf"] = "model/gltf+json";

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                   Path.Combine(Directory.GetCurrentDirectory(), "Assets/millennium_falcon")),
                RequestPath = "/Assets/millennium_falcon",
                ContentTypeProvider = provider
            });

            app.UseAuthentication();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            app.UseRouting();
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}");
            app.Run();
        }
    }
}
