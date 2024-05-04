using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {


        //public AppDbContext()
        //{

        //}


        //public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        //{

        //}

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<ApplicationUser> Accounts { get; set; }
        public DbSet<GameBoard> GameBoards { get; set; }
        public DbSet<Move> Moves { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameBoard>()
                .HasMany(g => g.Moves)
                .WithOne(m => m.GameBoard)
                .HasForeignKey(m => m.GameBoardID);

            modelBuilder.Entity<GameBoard>()
                .HasOne(g => g.PlayerOne)
                .WithMany()
                .HasForeignKey(g => g.PlayerOneID)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GameBoard>()
                .HasOne(g => g.PlayerTwo)
                .WithMany()
                .HasForeignKey(g => g.PlayerTwoID)
            .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<GameBoard>()
            //    .HasMany(g => g.Players)
            //    .WithMany();


            modelBuilder.Entity<Move>()
                .HasOne(m => m.Player)
                .WithMany()
                .HasForeignKey(m => m.PlayerID);














            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = "1",
                    UserName = "TheLegend27",
                    Email = "abc@abc.com",
                    PasswordHash = "passwort123",

                },
                                new ApplicationUser
                                {
                                    Id = "2",
                                    UserName = "DjBobo1337",
                                    Email = "bobo@abc.com",
                                    PasswordHash = "wertwert"
                                },
                                new ApplicationUser
                                {
                                    Id = "3",
                                    UserName = "FBeutlin69",
                                    Email = "Frodo@abc.com",
                                    PasswordHash
                                    = "qwert789"
                                },
                                new ApplicationUser
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
