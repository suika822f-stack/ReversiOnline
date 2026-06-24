using ReversiOnline.Services;

namespace ReversiOnline.Models;

public class Room
{
    public string RoomId { get; set; } = "";

    public ReversiGame Game { get; set; } = new();

    public List<PlayerInfo> Players { get; set; } = new();

    public class PlayerInfo
    {
        public string ConnectionId { get; set; } = "";

        public Stone Color { get; set; }
    }
}
