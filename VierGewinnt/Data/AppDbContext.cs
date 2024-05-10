using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<ApplicationUser> Accounts { get; set; }
        public DbSet<GameBoard> GameBoards { get; set; }
        public DbSet<Move> Moves { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ConstraintForeignkey auf Move mal anschauen und gegebenenfalls löschen.
            //    .WithMany()
            //    .HasForeignKey(g => g.PlayerOneID)
            //.OnDelete(DeleteBehavior.Cascade);

            //modelBuilder.Entity<GameBoard>()
            //    .HasOne(g => g.PlayerTwo)
            //    .WithMany()
            //    .HasForeignKey(g => g.PlayerTwoID)
            //    .OnDelete(DeleteBehavior.Cascade);
            //    .WithMany()
            //    .HasForeignKey(g => g.PlayerTwoID)
            //.OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameBoard>()
                .HasMany(g => g.Moves)
                .WithOne(m => m.GameBoard)
                .HasForeignKey(m => m.GameBoardID)
                .IsRequired();

            modelBuilder.Entity<Move>()
                .HasOne(m => m.Player)
                .WithMany()
                .HasForeignKey(m => m.PlayerID);
        }
    }
}
