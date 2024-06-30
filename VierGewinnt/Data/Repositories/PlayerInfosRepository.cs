using Microsoft.EntityFrameworkCore;
using VierGewinnt.Data.Interfaces;
using VierGewinnt.Data.Model;

namespace VierGewinnt.Data.Repositories
{
    public class PlayerInfosRepository : IPlayerInfoRepository
    {
        private readonly AppDbContext _context;

        public PlayerInfosRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> AddAsync(PlayerRanking item)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(PlayerRanking item)
        {
            throw new NotImplementedException();
        }

        public Task<List<PlayerRanking>> GetAllAsync()
        {
            return _context.PlayerRankings.OrderBy(pr => pr.Wins).ToListAsync();
        }

        public Task<PlayerRanking> GetByIdAsync(PlayerRanking item)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(PlayerRanking item)
        {
            throw new NotImplementedException();
        }
    }
}
