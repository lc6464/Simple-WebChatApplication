using Microsoft.AspNetCore.SignalR;

namespace SignalRWebpack.Hubs;

public class ChatHub : Hub {
	public async Task NewMessage(string username, string message) =>
		await Clients.All.SendAsync("messageReceived", username, message);

	/*
	public async Task JoinGroup(string name) {
		
		await Groups.AddToGroupAsync(Context.ConnectionId, name);
		await Clients.Caller.SendAsync("messageReceived", "Server", "You are in the group!");
		
	}
	*/
}