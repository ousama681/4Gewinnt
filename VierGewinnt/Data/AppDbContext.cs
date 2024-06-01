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
        public DbSet<PlayerRanking> PlayerRankings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameBoard>()
                .HasMany(g => g.Moves)
                .WithOne()
                .HasForeignKey(m => m.GameBoardID);

            modelBuilder.Entity<Move>()
                .HasOne(m => m.Player)
                .WithMany()
                .HasForeignKey(m => m.PlayerID);

            modelBuilder.Entity<PlayerRanking>()
                .HasOne(pr => pr.Player)
                .WithMany()
                .HasForeignKey(pr => pr.PlayerID);
        }
    }
}
