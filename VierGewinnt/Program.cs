using VierGewinnt.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using VierGewinnt.Hubs;
using VierGewinnt.Data.Repositories;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Data;
using VierGewinnt.Models;
using VierGewinnt.Data.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace VierGewinnt
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(DbUtility.connectionString));

            ConfigureIdentityOptions(builder);

            builder.Services.Configure<SMTPConfigModel>(builder.Configuration.GetSection("SMTPConfig"));
            builder.Services.AddScoped<IGameRepository, GameRepository>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IPlayerInfoRepository, PlayerInfosRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailService, EmailService>();


            // Adds Controllers
            builder.Services.AddControllersWithViews();

            //Adds SignalR
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();


            app.UseRouting();
            app.UseAuthorization();

            //Vorerst auskommentiert da das Laden damit viel länger dauert und das testen so auch länger.
            //------------
            // For 3D Homepage,  Set up custom content types - associating file extension to MIME type
            FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();

            // The MIME type for .GLB and .GLTF files are registered with IANA under the 'model' heading
            // https://www.iana.org/assignments/media-types/media-types.xhtml#model
            provider.Mappings[".glb"] = "model/gltf+binary";
            provider.Mappings[".gltf"] = "model/gltf+json";

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                   Path.Combine(Directory.GetCurrentDirectory(), "Assets/roboking")),
                RequestPath = "/Assets/roboking",
                ContentTypeProvider = provider
            });

            // Default ControllerMapping
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}");
            // Default ControllerMapping End

            // SignalR Hub Mapping
            app.MapHub<PlayerlobbyHub>("/playerlobbyHub");
            app.MapHub<GameHub>("/gameHub");
            app.MapHub<BoardEvEHub>("/boardEvEHub");
            app.MapHub<BoardPvEHub>("/boardPvEHub");


            //SignalR Hub Mapping End

            app.Run();
        }

        public static void ConfigureIdentityOptions(WebApplicationBuilder builder)
        {
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(5);
            });

            builder.Services.ConfigureApplicationCookie(config =>
            {
                IConfiguration _conf = builder.Configuration;
                config.LoginPath = _conf["Application:LoginPath"];
            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 5;
                //options.Password.RequiredUniqueChars = 1;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedEmail = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20);
                options.Lockout.MaxFailedAccessAttempts = 10;
            });

            return;
        }
    }
}


// For later Use

// Vorerst auskommentiert da das Laden damit viel länger dauert und das testen so auch länger.
//------------
//// For 3D Homepage,  Set up custom content types - associating file extension to MIME type
//FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();

//// The MIME type for .GLB and .GLTF files are registered with IANA under the 'model' heading
//// https://www.iana.org/assignments/media-types/media-types.xhtml#model
//provider.Mappings[".glb"] = "model/gltf+binary";
//provider.Mappings[".gltf"] = "model/gltf+json";

//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(
//       Path.Combine(Directory.GetCurrentDirectory(), "Assets/roboking")),
//    RequestPath = "/Assets/roboking",
//    ContentTypeProvider = provider
//});