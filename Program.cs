using VierGewinnt.Models;
using VierGewinnt.Repositories.Interfaces;
using VierGewinnt.Repositories;
using VierGewinnt.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace VierGewinnt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer("Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"));

            //builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            //    .AddEntityFrameworkStores<AppDbContext>()
            //    .AddDefaultTokenProviders();

            builder.Services.AddIdentity<Account, IdentityRole>()
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
                options.Lockout.MaxFailedAccessAttempts = 3;
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




            //builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            //{
            //    options.Password.RequiredLength = 10;
            //    options.Password.RequiredUniqueChars = 3;
            //    options.SignUp.RequireConfirmedEmail = true;
            //}).AddEntityFrameworkStores<AppDbContext>()
            //.AddDefaultTokenProviders();

            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailService, EmailService>();

            //builder.Services.AddScoped<IUserClaimsPrincipalFactory<Account>, AccountClaimsPrincipalFactory>();




            //Add services to the container.
            //builder.Services.AddControllersWithViews(config =>
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //                    .RequireAuthenticatedUser()
            //                    .Build();
            //    config.Filters.Add(new AuthorizeFilter(policy));
            //});           

            builder.Services.AddControllersWithViews();

            //builder.Services.AddMvc(config => {
            //    var policy = new AuthorizationPolicyBuilder()
            //                    .RequireAuthenticatedUser()
            //                    .Build();
            //    config.Filters.Add(new AuthorizeFilter(policy));
            //});

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseAuthorization();


            app.UseRouting();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}");

            app.Run();
        }
    }
}
