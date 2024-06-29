using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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

        public async Task<bool> AddAsync(GameBoard item)
        {
            await _context.GameBoards.AddAsync(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> DeleteAsync(GameBoard item)
        {
            throw new NotImplementedException();
        }

        public Task<List<GameBoard>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<GameBoard> GetByIdAsync(GameBoard item)
        {
            return await _context.GameBoards.Include(gb => gb.Moves).FirstAsync(gb => gb.ID.Equals(item.ID));
        }

        public Task UpdateAsync(GameBoard item)
        {
            throw new NotImplementedException();
        }

        public async Task AddMoveAsync(Move move)
        {
            await _context.Moves.AddAsync(move);
            await _context.SaveChangesAsync();
        }

        //public async Task AddGameBoardAsync(GameBoard board)
        //{
        //    await _context.GameBoards.AddAsync(board);
        //    await _context.SaveChangesAsync();
        //}

        public void Update(GameBoard item)
        {
            _context.GameBoards.Update(item);
            _context.SaveChanges();
        }

        public async Task<GameBoard> FindGameByPlayerNames(string playerOne, string playerTwo)
        {
            string playerOneID = _context.Accounts.FirstAsync(a => a.UserName.Equals(playerOne)).Result.Id;
            string playerTwoID = _context.Accounts.FirstAsync(a => a.UserName.Equals(playerTwo)).Result.Id;

            //return _context.GameBoards.Include(gb => gb.Moves).Single(gb =>
            //    gb.PlayerOneID.Equals(playerOneID) &&
            //    gb.PlayerTwoID.Equals(playerTwoID) &&
            //gb.IsFinished.Equals(0));

            return await _context.GameBoards.Include(gb => gb.Moves).FirstAsync(gb =>
                gb.PlayerOneID.Equals(playerOneID) &&
                gb.PlayerTwoID.Equals(playerTwoID));
            //&&
            //gb.IsFinished.Equals(0));
        }

        public Task<List<GameBoard>> FindGamesByPlayerName(string playerName)
        {
            string playerId = _context.Accounts.Where(user => user.UserName.Equals(playerName)).SingleOrDefault().Id;

            if (playerId == null)
            {

                string message = "Player with name " + playerName + "  doesnt exist!!!";
                // hier eventuell exception schmeissen
            }


            return _context.GameBoards.Include(gb => gb.Moves).Where(gb => (gb.PlayerOneID.Equals(playerId) || gb.PlayerTwoID.Equals(playerId)) && gb.IsFinished.Equals(true)).ToListAsync();
        }
    }
}
