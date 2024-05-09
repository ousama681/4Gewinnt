using VierGewinnt.Data.Models;

namespace VierGewinnt.Data.Interfaces
{
    public interface IGameRepository : IRepository<GameBoard>
    {
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
    }
}
