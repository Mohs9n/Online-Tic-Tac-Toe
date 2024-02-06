using Microsoft.AspNetCore.SignalR;

namespace server.Hubs;

public class XoHub : Hub
{
    private static Dictionary<int, List<string>> connectedIds = [];
    private static Dictionary<int, string> _boards = [];

    public override async Task OnConnectedAsync()
    {
        string path = Context.GetHttpContext().Request.Path;
        var splitPath = path.Split('/');
        string gameIdString = splitPath[^1];
        int gameId = int.Parse(gameIdString);
        Console.WriteLine(gameId);
        if (_boards.ContainsKey(gameId))
            Console.WriteLine($"Board in Connect Before: ={_boards[gameId]}");
        if (_boards.ContainsKey(gameId))
        {

        }
        else
        {
            _boards.Add(gameId, "         X");
        }

        if (connectedIds.ContainsKey(gameId)) connectedIds[gameId].Add(Context.ConnectionId);
        else
            connectedIds.Add(gameId, [Context.ConnectionId]);

        if (connectedIds.ContainsKey(gameId))
        {
            Console.WriteLine("Count: {0}", connectedIds[gameId].Count);
            if (connectedIds[gameId].Count == 1)
                await Clients.Caller.SendAsync("ReceiveSide", "X");
            else if (connectedIds[gameId].Count == 2)
                await Clients.Caller.SendAsync("ReceiveSide", "O");
            else await Clients.Caller.SendAsync("ReceiveSide", "S");
            await Clients.Caller.SendAsync("ReceiveBoard", _boards[gameId]);
        }

        if (_boards.ContainsKey(gameId))
            Console.WriteLine($"Board in Connect after: ={_boards[gameId]}");
        await base.OnConnectedAsync();
    }
    public async Task ResetBoard(int roomId)
    {
        Console.WriteLine("Reset Board");
        _boards[roomId]="         X";
        await SendBoard(_boards[roomId]);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string path = Context.GetHttpContext().Request.Path;
        var splitPath = path.Split('/');
        string gameIdString = splitPath[^1];
        int gameId = int.Parse(gameIdString);
        if (connectedIds.ContainsKey(gameId))
        {
            if (connectedIds[gameId].Count == 1)
            {
                connectedIds.Remove(gameId);
                _boards.Remove(gameId);
                Console.WriteLine("In if");
            }
            else
            {
                connectedIds[gameId].Remove(Context.ConnectionId);
                Clients.Clients(connectedIds[gameId]).SendAsync("OpponentDisconnected");
                Console.WriteLine("In else");
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    // public Dictionary<int, string> GetConnectedUsers()
    // {
    //     // You might want to filter or process the IDs further here
    //     return connectedIds;
    // }

    public async Task SendBoard(string board)
    {
        Console.WriteLine($"Board in SendBoard ={board}");
        string path = Context.GetHttpContext().Request.Path;
        var splitPath = path.Split('/');
        string gameIdString = splitPath[^1];
        int gameId = int.Parse(gameIdString);
        _boards[gameId] = board;
        Console.WriteLine(CheckWin(board));
        var gameResult = CheckWin(_boards[gameId]);
        switch (gameResult)
        {
            case "": break;
            case "X":
                await Clients.Clients(connectedIds[gameId]).SendAsync("ReceiveWinner", "X");
                break;
            case "O":
                await Clients.Clients(connectedIds[gameId]).SendAsync("ReceiveWinner", "O");
                break;
            default:
                break;
        }
        await Clients.Clients(connectedIds[gameId]).SendAsync("ReceiveBoard", _boards[gameId]);
    }

    private static string CheckWin(string board)
    {
        // Define winning rows and diagonals
        char[][] winningLines =
        [
        [board[0], board[1], board[2]],
        [board[3], board[4], board[5]],
        [board[6], board[7], board[8]],
        [board[0], board[3], board[6]],
        [board[1], board[4], board[7]],
        [board[2], board[5], board[8]],
        [board[0], board[4], board[8]],
        [board[2], board[4], board[6]]
        ];

        // Check for wins on each line
        foreach (char[] line in winningLines)
        {
            char symbol = line[0];
            if (symbol != ' ' && symbol == line[1] && symbol == line[2])
            {
                return symbol.ToString();
            }
        }

        // Check for tie
        if (!board.Contains(' '))
        {
            return "Tie";
        }

        // No winner yet
        return "";
    }

}
