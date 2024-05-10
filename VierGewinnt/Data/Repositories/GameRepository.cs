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

        public bool AddAsync(GameBoard item)
        {
            _context.GameBoards.Add(item);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteAsync(GameBoard item)
        {
            throw new NotImplementedException();
        }

        public List<GameBoard> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public GameBoard GetByIdAsync(GameBoard item)
        {
            return _context.GameBoards.Include(gb => gb.Moves).FirstAsync(gb => gb.ID.Equals(item.ID)).Result;
        }

        public Task UpdateAsync(GameBoard item)
        {
            throw new NotImplementedException();
        }

        public void AddMoveAsync(Move move)
        {
            _context.Moves.Add(move);
            _context.SaveChanges();
        }

        public void AddGameBoardAsync(GameBoard board)
        {
            _context.GameBoards.Add(board);
            _context.SaveChanges();
        }

        Task IRepository<GameBoard>.UpdateAsync(GameBoard item)
        {
            throw new NotImplementedException();
        }

        public GameBoard FindGameByPlayerNames(string playerOne, string playerTwo)
        {
            string playerOneID = _context.Accounts.First(a => a.UserName.Equals(playerOne)).Id;
            string playerTwoID = _context.Accounts.First(a => a.UserName.Equals(playerTwo)).Id;

            //return _context.GameBoards.Include(gb => gb.Moves).Single(gb =>
            //    gb.PlayerOneID.Equals(playerOneID) &&
            //    gb.PlayerTwoID.Equals(playerTwoID) &&
            //gb.IsFinished.Equals(0));

            return _context.GameBoards.Include(gb => gb.Moves).First(gb =>
                gb.PlayerOneID.Equals(playerOneID) &&
                gb.PlayerTwoID.Equals(playerTwoID));
            //&&
            //gb.IsFinished.Equals(0));
        }
    }
}
