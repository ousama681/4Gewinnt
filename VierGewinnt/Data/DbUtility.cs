using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Data
{
    public static class DbUtility
    {
        // "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"
        // "Server=Koneko\\KONEKO;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;"

        public static string connectionString = "Server=DESKTOP-PMVN625;Database=4Gewinnt;Trusted_connection=True;TrustServerCertificate=True;";



        public static ApplicationUser GetUser(string playerOne)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            using (AppDbContext dbContext = new AppDbContext(optionsBuilder.Options))
            {
                try
                {
                    var userOne = GetUser(playerOne, dbContext).Result;
                    return userOne;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
            return null;
        }

        public static async Task<ApplicationUser> GetUser(string playerName, AppDbContext context)
        {
            return context.Accounts.Where(u => u.UserName.Equals(playerName)).FirstOrDefault();
        }
    }
}
