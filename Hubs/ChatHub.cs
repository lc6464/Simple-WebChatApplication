using Microsoft.AspNetCore.SignalR;

namespace SignalRWebpack.Hubs;

public class ChatHub : Hub {
	private Group? JointGroup {
		
		//get => Cache.MemoryCache.Get<Group>($"ChatHub JointGroup of {Context.ConnectionId}");
		get {
			var result = Cache.MemoryCache.Get<Group>($"ChatHub JointGroup of {Context.ConnectionId}");
			Console.WriteLine(result?.Name ?? "<null>");
			Console.WriteLine(Context.ConnectionId);
			
			return result;
		}
		set {
			if (value is null) {
				Cache.MemoryCache.Remove($"ChatHub JointGroup of {Context.ConnectionId}");
			} else {
				Console.WriteLine(Cache.Set($"ChatHub JointGroup of {Context.ConnectionId}", value, TimeSpan.FromHours(1)).Name);
				Console.WriteLine(Context.ConnectionId);
			}
		}
	}


	public async Task NewMessage(string username, string message) =>
		await (JointGroup is null ?
			Clients.Caller.SendAsync("groupResult", "sendFailed", "warning", "您尚未加入任何群组。") :
			Clients.Group(JointGroup.Name).SendAsync("messageReceived", username, message));


	public async Task JoinGroup(string name, string password) {
		if (!Cache.MemoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) || !groups!.Any(group => group.Name == name)) {
			await Clients.Caller.SendAsync("groupResult", "joinFailed", "warning", "此群组不存在！");
			return;
		}
		
		
		var group = groups!.First(group => group.Name == name);
		if (!group.VerifyPassword(password)) {
			await Clients.Caller.SendAsync("groupResult", "joinFailed", "error", "密码错误！");
			return;
		}

		JointGroup = group;
		lock (group) {
			group.MemberCount++;
		}
		
		await Groups.AddToGroupAsync(Context.ConnectionId, name);
		await Clients.Caller.SendAsync("groupResult", "joinSuccess", "success", "加入群组成功！");
		await Clients.OthersInGroup(name).SendAsync("messageReceived", "Server", $"{Context.ConnectionId} 已加入群组！");

	}

	public async Task LeaveGroup() {
		if (JointGroup is null) {
			await Clients.Caller.SendAsync("groupResult", "leaveFailed", "warning", "你没有加入任何群组！");
			return;
		}

		lock (JointGroup) {
			JointGroup.MemberCount--;
			if (JointGroup.MemberCount == 0) {
				var groups = Cache.MemoryCache.Get<List<Group>>("ChatHub Groups")!;
				lock (groups) {
					groups.Remove(JointGroup);
				}
			}
		}

		await Clients.OthersInGroup(JointGroup.Name).SendAsync("messageReceived", "Server", $"{Context.ConnectionId} 已离开群组！");
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, JointGroup.Name);
		await Clients.Caller.SendAsync("groupResult", "leaveSuccess", "success", "你已经成功离开群组！");

		JointGroup = null;
	}

	public async Task CreateGroup(string name, string password) {
		if (Cache.MemoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) && groups!.Any(group => group.Name == name)) {
			await Clients.Caller.SendAsync("groupResult", "createFailed", "error", "此群组已存在！请更换群组名称！");
			return;
		}

		var group = new Group(name, password) { MemberCount = 1 };
		if (groups is null) {
			Cache.MemoryCache.Set("ChatHub Groups", new List<Group> { group });
		} else {
			lock (groups) {
				groups.Add(group);
				// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
				Cache.Set("ChatHub Groups", groups, TimeSpan.FromHours(2));
			}
		}

		JointGroup = group;

		await Groups.AddToGroupAsync(Context.ConnectionId, name);
		await Clients.Caller.SendAsync("groupResult", "createSuccess", "success", "你已经成功创建群组！");
	}

	public override async Task OnDisconnectedAsync(Exception? exception) {
		if (JointGroup is not null) {
			lock (JointGroup) {
				JointGroup.MemberCount--;
				if (JointGroup.MemberCount == 0) {
					var groups = Cache.MemoryCache.Get<List<Group>>("ChatHub Groups")!;
					lock (groups) {
						groups.Remove(JointGroup);
					}
				}
			}
			JointGroup = null;
		}
		
		await base.OnDisconnectedAsync(exception);
	}

}