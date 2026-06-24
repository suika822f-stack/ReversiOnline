using Microsoft.AspNetCore.SignalR;
using ReversiOnline.Models;
using ReversiOnline.Services;
using static ReversiOnline.Models.Room;

namespace ReversiOnline.Hubs;

public class ReversiHub : Hub
{
    private static Dictionary<string, Room> Rooms = new();

    public async Task<string> CreateRoom()
    {
        string roomId = Random.Shared.Next(1000, 9999).ToString();

        var room = new Room
        {
            RoomId = roomId,
            Game = new ReversiGame()
        };

        room.Players.Add(new PlayerInfo
        {
            ConnectionId = Context.ConnectionId,
            Color = Stone.Black
        });

        Rooms.Add(roomId, room);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            roomId);

        await Clients.Caller
        .SendAsync("AssignColor", "Black");

        await Clients.Group(roomId)
            .SendAsync("BoardUpdated", room.Game.GetState());

        return roomId;
    }
    public async Task<bool> JoinRoom(string roomId)
    {
        if (!Rooms.TryGetValue(roomId, out var room))
            return false;

        room.Players.Add(new PlayerInfo
        {
            ConnectionId = Context.ConnectionId,
            Color = Stone.White
        });

        if (room.Players.Count == 1)
        {
            await Clients.Caller.SendAsync("AssignColor", "Black");
        }
        else if (room.Players.Count == 2)
        {
            await Clients.Caller.SendAsync("AssignColor", "White");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            roomId);

        await Clients.Caller
        .SendAsync("AssignColor", "White");

        await Clients.Group(roomId)
            .SendAsync("BoardUpdated", room.Game.GetState());

        return true;
    }
    public async Task PlaceStone(string roomId, int x, int y)
    {
        if (!Rooms.TryGetValue(roomId, out var room))
        {
            return;
        }

        var player = room.Players
        .FirstOrDefault(
            p => p.ConnectionId == Context.ConnectionId);

        if (player is null)
        {
            return;
        }

        if (player.Color != room.Game.CurrentPlayer)
        {
            return;
        }

        room.Game.PlaceStone(x, y);

        // ④ 送信
        await Clients.Group(roomId)
            .SendAsync("BoardUpdated", room.Game.GetState());
    }
}
