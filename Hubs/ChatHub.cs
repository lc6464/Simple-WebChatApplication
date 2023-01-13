using Microsoft.AspNetCore.SignalR;

namespace SignalRWebpack.Hubs;

public class ChatHub : Hub {
	private readonly IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
	private Group? jointGroup;
	

	public async Task NewMessage(string username, string message) =>
		await Clients.All.SendAsync("messageReceived", username, message);


	public async Task JoinGroup(string name, string password) {
		if (!memoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) || !groups!.Any(group => group.Name == name)) {
			await Clients.Caller.SendAsync("groupResult", "joinFailed", "warning", "此群组不存在！");
			return;
		}
		
		
		var group = groups!.First(group => group.Name == name);
		if (!group.VerifyPassword(password)) {
			await Clients.Caller.SendAsync("groupResult", "joinFailed", "error", "密码错误！");
			return;
		}

		jointGroup = group;
		lock (group) {
			group.MemberCount++;
		}
		
		await Groups.AddToGroupAsync(Context.ConnectionId, name);
		await Clients.Caller.SendAsync("messageReceived", "Server", "You are in the group!");
		await Clients.OthersInGroup(name).SendAsync("messageReceived", "Server", $"{Context.ConnectionId} has joined the group!");

	}

	public async Task LeaveGroup() {
		if (jointGroup is null) {
			await Clients.Caller.SendAsync("groupResult", "leaveFailed", "warning", "你没有加入任何群组！");
			return;
		}

		lock (jointGroup) {
			jointGroup.MemberCount--;
			if (jointGroup.MemberCount == 0) {
				var groups = memoryCache.Get<List<Group>>("ChatHub Groups")!;
				lock (groups) {
					groups.Remove(jointGroup);
				}
			}
		}

		await Clients.OthersInGroup(jointGroup.Name).SendAsync("messageReceived", "Server", $"{Context.ConnectionId} 已离开群组！");
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, jointGroup.Name);
		await Clients.Caller.SendAsync("groupResult", "leaveSuccess", "success", "你已经成功离开群组！");

		jointGroup = null;
	}

	public async Task CreateGroup(string name, string password) {
		if (memoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) && groups!.Any(group => group.Name == name)) {
			await Clients.Caller.SendAsync("groupResult", "createFailed", "warning", "此群组已存在！请更换群组名称！");
			return;
		}

		var group = new Group(name, password);
		if (groups is null) {
			memoryCache.Set("ChatHub Groups", new List<Group> { group });
		} else {
			lock (groups) {
				groups.Add(group);
			}
		}

		await Groups.AddToGroupAsync(Context.ConnectionId, name);
		await Clients.Caller.SendAsync("groupResult", "createSuccess", "success", "你已经成功创建群组！");
	}

	public override async Task OnDisconnectedAsync(Exception? exception) {
		if (jointGroup is not null) {
			lock (jointGroup) {
				jointGroup.MemberCount--;
				if (jointGroup.MemberCount == 0) {
					var groups = memoryCache.Get<List<Group>>("ChatHub Groups")!;
					lock (groups) {
						groups.Remove(jointGroup);
					}
				}
			}
			jointGroup = null;
		}
		
		await base.OnDisconnectedAsync(exception);
	}

}