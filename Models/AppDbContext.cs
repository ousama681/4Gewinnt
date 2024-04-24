using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VierGewinnt.Models
{
    public class AppDbContext : IdentityDbContext
    {


        //public AppDbContext()
        //{

        //}


        //public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        //{

        //}

        public AppDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<IdentityUser> Accounts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUser>().HasData(
                new IdentityUser
                {
                    Id = "1",
                    UserName = "TheLegend27",
                    Email = "abc@abc.com",
                    PasswordHash = "passwort123",

                },
                                new IdentityUser
                                {
                                    Id = "2",
                                    UserName = "DjBobo1337",
                                    Email = "bobo@abc.com",
                                    PasswordHash = "wertwert"
                                },
                                new IdentityUser
                                {
                                    Id = "3",
                                    UserName = "FBeutlin69",
                                    Email = "Frodo@abc.com",
                                    PasswordHash
                                    = "qwert789"
                                },
                                new IdentityUser
                                {
                                    Id = "4",
                                    UserName = "Son_Goku",
                                    Email = "Frodo@abc.com",
                                    PasswordHash
                                    = "afasfwafafa"
                                }
                );
        }
    }
}
