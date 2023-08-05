﻿using Microsoft.AspNetCore.SignalR;
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
	
	//发送信息
	public async Task MessageAsync(string? echo) {
		if (JointGroup is null) {
			await Clients.Caller.SendAsync("notice", "string", "您尚未加入任何群组", "error");
		} else {
			await Clients.Group(JointGroup.Name).SendAsync("messageServer", DisplayName, echo, DateTime.Now.ToString("yyyy-M-d H:mm:ss"));
			await Clients.Caller.SendAsync("messageSelf", DateTime.Now.ToString("yyyy-M-d H:mm:ss"), echo);
		}
	}		
	// 加入/创建群组
	public async Task GroupEnterAsync(string type, string name, string password) {
		if ("create".Equals(type)) {
			//建群
			if (Cache.MemoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) && groups!.Exists(group => group.Name == name)) {
				//群已存在
				await Clients.Caller.SendAsync("groupEnter", "eFailed");
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
			//群组创建成功
			await Clients.Caller.SendAsync("groupEnter", "jSuccess");
		} else if ("join".Equals(type)) {
			//入群
			if (!Cache.MemoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) || !groups!.Exists(group => group.Name == name)) {
				//群不存在
				await Clients.Caller.SendAsync("groupEnter", "nFailed");
				return;
			}
			var group = groups!.First(group => group.Name == name);
			if (!group.VerifyPassword(password)) {
				//密码错误
				await Clients.Caller.SendAsync("groupEnter", "pwdError");
				return;
			}
			JointGroup = group;
			lock (group) {
				group.MemberCount++;
			}
			await Groups.AddToGroupAsync(Context.ConnectionId, name);
			//入群成功
			await Clients.Caller.SendAsync("groupEnter", "cSuccess");
			await Clients.OthersInGroup(name).SendAsync("messageServer", "Server", $"{DisplayName} 已加入群组！", DateTime.Now.ToString("yyyy-M-d H:mm:ss"));
		} else {
			//type非指定的参数(create|join)
			await Clients.Caller.SendAsync("notice", "string", $"未知参数{type}", "error");
		}
	}
	
	//离开群组
	public async Task GroupLeaveAsync() {
		if (JointGroup is null) {
			await Clients.Caller.SendAsync("notice", "string", "您尚未加入任何群组", "error");
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

		await Clients.OthersInGroup(JointGroup.Name).SendAsync("messageServer", "Server", $"{DisplayName} 已离开群组。", DateTime.Now.ToString("yyyy-M-d H:mm:ss"));
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, JointGroup.Name);
		await Clients.Caller.SendAsync("notice",  "string", "你已经成功离开群组！", "success");

		JointGroup = null;
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

			await Clients.OthersInGroup(JointGroup.Name).SendAsync("messageServer", "Server", $"{DisplayName} 已离开群组。", DateTime.Now.ToString("yyyy-M-d H:mm:ss"));
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, JointGroup.Name);
			JointGroup = null;
		}


		await base.OnDisconnectedAsync(exception);
	}

	public override async Task OnConnectedAsync() {
		var context = Context.GetHttpContext();
		if (context is null) {
			await base.OnConnectedAsync();
			Context.Abort();
			return;
		}
		_httpContext = context;
		if (!_tools.IsLogin()) {
			await base.OnConnectedAsync();
			Context.Abort();
			return;
		}
		await Clients.Caller.SendAsync("groupResult", "Welcome",  "连接成功！", "success");
		await base.OnConnectedAsync();
	}
}