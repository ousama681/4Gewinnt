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

            //modelBuilder.Entity<Account>().HasData(
            //    new Account
            //    {
            //        Id = "1",
            //        UserName = "TheLegend27",
            //        Email = "abc@abc.com",
            //        PasswordHash = "passwort123",
                    
            //    },
            //                    new Account
            //                    {
            //                        Id = "2",
            //                        UserName = "DjBobo1337",
            //                        Email = "bobo@abc.com",
            //                        PasswordHash = "wertwert"
            //                    },
            //                    new Account
            //                    {
            //                        Id = "3",
            //                        UserName = "FBeutlin69",
            //                        Email = "Frodo@abc.com",
            //                        PasswordHash
            //                        = "qwert789"
            //                    }
            //    );
        }
    }
}
