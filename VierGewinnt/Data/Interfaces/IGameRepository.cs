using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Data.Interfaces
{
    public interface IGameRepository : IRepository<GameBoard>
    {
        //Task<bool> AddAsync(GameBoard item);
        Task<bool> DeleteAsync(GameBoard item);
        Task<List<GameBoard>> GetAllAsync();
        Task<GameBoard> GetByIdAsync(GameBoard item);
        void Update(GameBoard item);
        Task AddMoveAsync(Move move);
        //Task AddGameBoardAsync(GameBoard board);
        Task<GameBoard> FindGameByPlayerNames(string playerOne, string playerTwo);
        Task<List<GameBoard>> FindGamesByPlayerName(string playerName);
    }
}
