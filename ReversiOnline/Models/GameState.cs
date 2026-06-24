using ReversiOnline.Models;

namespace ReversiOnline.Models;

public class GameState
{
    public Stone[] Board { get; set; } = new Stone[64];

    public Stone CurrentPlayer { get; set; }

    public string ResultMessage { get; set; } = "";
}