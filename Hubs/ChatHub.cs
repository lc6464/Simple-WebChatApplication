using Microsoft.AspNetCore.SignalR;
using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Hubs;

public class ChatHub : Hub {
	private readonly ICheckingTools _tools;
	private HttpContext? _httpContext;
	private string DisplayName => $"{_httpContext?.Session.GetString("Nick")} ({_httpContext?.Session.GetString("Name")})";

	public ChatHub(ICheckingTools checkingTools) => _tools = checkingTools;


	private Group? JointGroup {
		get => Cache.MemoryCache.Get<Group>($"ChatHub JointGroup of {Context.ConnectionId}");
		set {
			if (value is null) {
				Cache.MemoryCache.Remove($"ChatHub JointGroup of {Context.ConnectionId}");
			} else {
				_ = Cache.Set($"ChatHub JointGroup of {Context.ConnectionId}", value, TimeSpan.FromHours(1));
			}
		}
	}

	public async Task SendMessageAsync(string message) =>
		await (JointGroup is null ?
			Clients.Caller.SendAsync("groupResult", "sendFailed", "warning", "您尚未加入任何群组。") :
			Clients.Group(JointGroup.Name).SendAsync("messageReceived", DisplayName, message, DateTime.Now.ToString("yyyy-M-d H:mm:ss")));


	public async Task JoinGroupAsync(string name, string password) {
		if (!Cache.MemoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) || !groups!.Exists(group => group.Name == name)) {
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
		await Clients.OthersInGroup(name).SendAsync("messageReceived", "Server", $"{DisplayName} 已加入群组！");

	}

	public async Task LeaveGroupAsync() {
		if (JointGroup is null) {
			await Clients.Caller.SendAsync("groupResult", "leaveFailed", "warning", "你没有加入任何群组！");
			return;
		}

		lock (JointGroup) {
			JointGroup.MemberCount--;
			if (JointGroup.MemberCount == 0) {
				var groups = Cache.MemoryCache.Get<List<Group>>("ChatHub Groups")!;
				lock (groups) {
					_ = groups.Remove(JointGroup);
				}
			}
		}

		await Clients.OthersInGroup(JointGroup.Name).SendAsync("messageReceived", "Server", $"{DisplayName} 已离开群组！");
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, JointGroup.Name);
		await Clients.Caller.SendAsync("groupResult", "leaveSuccess", "success", "你已经成功离开群组！");

		JointGroup = null;
	}

	public async Task CreateGroupAsync(string name, string password) {
		if (Cache.MemoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) && groups!.Exists(group => group.Name == name)) {
			await Clients.Caller.SendAsync("groupResult", "createFailed", "error", "此群组已存在！请更换群组名称！");
			return;
		}

		var group = new Group(name, password) { MemberCount = 1 };
		if (groups is null) {
			_ = Cache.Set("ChatHub Groups", new List<Group> { group }, TimeSpan.FromHours(2));
		} else {
			lock (groups) {
				groups.Add(group);
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
						_ = groups.Remove(JointGroup);
					}
				}
			}

			await Clients.OthersInGroup(JointGroup.Name).SendAsync("messageReceived", "Server", $"{DisplayName} 已离开群组！");
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, JointGroup.Name);
			JointGroup = null;
		}


		await base.OnDisconnectedAsync(exception);
	}

	public override async Task OnConnectedAsync() {
		var context = Context.GetHttpContext();
		if (context is null) {
			await Clients.Caller.SendAsync("groupResult", "connectFailed", "error", "无法获取 HttpContext！");
			Context.Abort();
			return;
		}
		_httpContext = context;
		/*if (!_tools.IsLogin()) {
			await Clients.Caller.SendAsync("groupResult", "connectFailed", "error", "你尚未登录！");
			Context.Abort();
			return;
		}*/
		await Clients.Caller.SendAsync("groupResult", "Welcome", "success", $"连接成功！{_httpContext is null}");
		await base.OnConnectedAsync();
	}
}