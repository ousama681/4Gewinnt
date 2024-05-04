using Microsoft.AspNetCore.SignalR;
namespace VierGewinnt.Hubs
{
    public class GameHub : Hub
    {
        public string playerOne { get; set; }
        public string playerTwo { get; set; }


        public void PlaceYellowChip(int col)
        {

        }

        public void PlaceRedChip(int col)
        {

        }
    }
}
