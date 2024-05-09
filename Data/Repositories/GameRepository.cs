using VierGewinnt.Data.Interfaces;
using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Data.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;
        public GameRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<bool> AddAsync(GameBoard item)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(GameBoard item)
        {
            throw new NotImplementedException();
        }

        public Task<List<GameBoard>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GameBoard> GetByIdAsync(GameBoard item)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(GameBoard item)
        {
            throw new NotImplementedException();
        }

        public async Task AddMoveAsync(Move move)
        {
            _context.Moves.Add(move);
            await _context.SaveChangesAsync();
        }

        public async Task AddGameBoardAsync(GameBoard board)
        {
            _context.GameBoards.Add(board);
            await _context.SaveChangesAsync();
        }

    }
}
