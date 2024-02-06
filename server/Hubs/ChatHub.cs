using Microsoft.AspNetCore.SignalR;

namespace server.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task SendArray(int[] arr)
    {
        Console.WriteLine(arr);
        foreach (var item in arr)
        {

            Console.WriteLine(item);
        }
        await Clients.All.SendAsync("ReceiveArray", arr);
    }
}
