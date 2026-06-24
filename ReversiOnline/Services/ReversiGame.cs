using ReversiOnline.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace ReversiOnline.Services;

public class ReversiGame
{
    public Cell[,] Board { get; } = new Cell[8, 8];
    public ObservableCollection<Cell> Cells { get; }
        = new ObservableCollection<Cell>();
    private bool isGameSet;
    public event PropertyChangedEventHandler? PropertyChanged;
    public bool IsGameSet
    {
        get => isGameSet;
        set
        {
            isGameSet = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGameSet)));
        }
    }
    int[,] direction = {{-1, -1}, {0, -1}, {1, -1},
                                {-1, 0}, {0, 0}, {1, 0},
                                {-1, 1}, {0, 1}, {1, 1} };
    List<(int x, int y)> reverseCells = new();
    public Stone[,] boardInfo = new Stone[8, 8];
    public Stone CurrentPlayer { get; set; } = Stone.Black;
    public string ResultMessage { get; set; } = "";

    public ReversiGame()
    {
        Initialize();
    }

    public void Initialize()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Board[x, y] = new Cell
                {
                    X = x,
                    Y = y,
                    Stone = Stone.Empty
                };
            }
        }

        Board[3, 3].Stone = Stone.White;
        Board[3, 4].Stone = Stone.Black;
        Board[4, 3].Stone = Stone.Black;
        Board[4, 4].Stone = Stone.White;

        PredictPlace();
    }

    public void PlaceStone(int x, int y)
    {
        if (!CanPlace(x, y))
        {
            return;
        }
        if (!CanReverse(x, y))
        {
            return;
        }

        PutStone(x, y);

        ReverseStone(x, y);

        if (!Judge())
        {
            ChangeTurn();

            PredictPlace();
        }
        else
        {
            DeletePredict();
        }
    }
    public bool CanPlace(int x, int y)
    {
        //すでに石がある場所には置けない
        if (Board[x, y].Stone == Stone.White || Board[x, y].Stone == Stone.Black)
        {
            return false;
        }
        return true;
    }
    bool CanReverse(int x, int y)
    {
        for (int i = 0; i <= 8; i++)
        {
            int dx = direction[i, 0];
            int dy = direction[i, 1];
            if (dx == 0 && dy == 0)
            {
                continue;
            }
            int checkx = x + dx;
            int checky = y + dy;
            Stone opponentStone = CurrentPlayer == Stone.Black
                                ? Stone.White
                                : Stone.Black;

            int count = 0;
            while (checkx >= 0 && checkx < 8 && checky >= 0 && checky < 8)
            {
                if (Board[checkx, checky].Stone == opponentStone)
                {
                    checkx += dx;
                    checky += dy;
                    count++;
                    continue;
                }
                else if (Board[checkx, checky].Stone == CurrentPlayer && count != 0)
                {
                    return true;
                }
                break;
            }
        }
        return false;
    }
    void PutStone(int x, int y)
    {
        Board[x, y].Stone = CurrentPlayer;
    }
    void ReverseStone(int x, int y)
    {
        for (int i = 0; i <= 8; i++)
        {
            int dx = direction[i, 0];
            int dy = direction[i, 1];
            if (dx == 0 && dy == 0)
            {
                continue;
            }

            CheckDirection(x, y, dx, dy);
        }

    }
    void CheckDirection(int x, int y, int dx, int dy)
    {
        int checkx = x + dx;
        int checky = y + dy;
        Stone opponentStone = CurrentPlayer == Stone.Black
                            ? Stone.White
                            : Stone.Black;

        while (checkx >= 0 && checkx < 8 && checky >= 0 && checky < 8)
        {
            if (Board[checkx, checky].Stone == opponentStone)
            {
                reverseCells.Add((checkx, checky));

                checkx += dx;
                checky += dy;
                continue;
            }
            if (Board[checkx, checky].Stone == CurrentPlayer)
            {
                foreach (var (rx, ry) in reverseCells)
                {
                    Board[rx, ry].Stone = CurrentPlayer;
                }
            }
            reverseCells.Clear();
            break;
        }
        reverseCells.Clear();
    }
    void PredictPlace()
    {
        DeletePredict();

        int count = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (Board[x, y].Stone == Stone.White || Board[x, y].Stone == Stone.Black)
                {
                    continue;
                }
                if (CanReverse(x, y))
                {
                    Board[x, y].Stone = Stone.Predict;
                    count++;
                }
            }

        }
        if (count == 0)
        {
            ResultMessage = "置くところがないのでパスします";
            ChangeTurn();
            PredictPlace();
        }
    }
    void DeletePredict()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (Board[x, y].Stone == Stone.Predict)
                {
                    Board[x, y].Stone = Stone.Empty;
                }
            }
        }
    }
    public void ChangeTurn()
    {
        CurrentPlayer = CurrentPlayer == Stone.Black
                        ? Stone.White
                        : Stone.Black;
    }
    void CountStone()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                boardInfo[x, y] = Board[x, y].Stone;
            }
        }
    }
    bool Judge()
    {
        CountStone();
        int countblack = 0;
        int countwhite = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (boardInfo[x, y] == Stone.Black)
                {
                    countblack++;
                }
                else if (boardInfo[x, y] == Stone.White)
                {
                    countwhite++;
                }
            }
        }
        if (countblack + countwhite == 64)
        {
            if (countblack > countwhite)
            {
                ResultMessage = $"黒: {countblack}個, 白: {countwhite}個\n黒の勝利！！";
            }
            else if (countblack < countwhite)
            {
                ResultMessage = $"黒: {countblack}個, 白: {countwhite}個\n白の勝利！！";
            }
            else
            {
                ResultMessage = "ひきわけ...";
            }
            IsGameSet = true;
            return true;
        }
        else
        {
            if (countwhite == 0)
            {
                ResultMessage = $"黒: {countblack}個, 白: {countwhite}個\n黒の勝利！！";
                IsGameSet = true;
                return true;
            }
            else if (countblack == 0)
            {
                ResultMessage = $"黒: {countblack}個, 白: {countwhite}個\n白の勝利！！";
                IsGameSet = true;
                return true;
            }
            return false;
        }
    }

    public GameState GetState()
    {
        var state = new GameState();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                state.Board[y * 8 + x] = Board[x, y].Stone;
            }
        }

        state.CurrentPlayer = CurrentPlayer;
        state.ResultMessage = ResultMessage;

        return state;
    }
}
