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

        public DbSet<Account> Accounts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    Id = "1",
                    PlayerName = "TheLegend27",
                    Email = "abc@abc.com",
                    Password = "passwort123"
                },
                                new Account
                                {
                                    Id = "2",
                                    PlayerName = "DjBobo1337",
                                    Email = "bobo@abc.com",
                                    Password = "wertwert"
                                },
                                new Account
                                {
                                    Id = "3",
                                    PlayerName = "FBeutlin69",
                                    Email = "Frodo@abc.com",
                                    Password = "qwert789"
                                }
                );
        }
    }
}
