using VierGewinnt.Data.Model;
using VierGewinnt.Data.Models;

namespace VierGewinnt.Data.Interfaces
{
    public interface IGameRepository : IRepository<GameBoard>
    {
        bool AddAsync(GameBoard item);
        bool DeleteAsync(GameBoard item);
        List<GameBoard> GetAllAsync();
        GameBoard GetByIdAsync(GameBoard item);
        Task UpdateAsync(GameBoard item);
        void AddMoveAsync(Move move);
        void AddGameBoardAsync(GameBoard board);
        GameBoard FindGameByPlayerNames(string playerOne, string playerTwo);
    }
}
