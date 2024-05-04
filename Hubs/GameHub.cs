using Microsoft.AspNetCore.SignalR;
namespace VierGewinnt.Hubs
{
    public class GameHub :Hub
    {
        public string Test {  get; set; }
    }
}
