﻿using Microsoft.AspNetCore.SignalR;
using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Hubs;
public class ChatHub : Hub {
	private readonly ICheckingTools _tools;
	private HttpContext? _httpContext;

	/// <summary>
	/// 显示的用户名称
	/// </summary>
	private string DisplayName => $"{_httpContext?.Session.GetString("Nick")} ({_httpContext?.Session.GetString("Name")})";

	/// <summary>
	/// 当前时间戳
	/// </summary>
	private static long Timestamp => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

	public ChatHub(ICheckingTools checkingTools) => _tools = checkingTools;

	/// <summary>
	/// 获取或设置当前加入的群组。
	/// </summary>
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


	// 发送信息
	public async Task MessageAsync(string? message, string? echo) {
		if (JointGroup is null) {
			await Clients.Caller.SendAsync("notice", "发送失败", "您尚未加入任何群组！", "error");
		} else if (!string.IsNullOrEmpty(message)) {
			if (message.Length > 100_000) { // 字数限制：0.1M
				await Clients.Caller.SendAsync("notice", "发送失败", "您发送的消息超过了长度限制！", "error");
				return;
			}
			await Clients.OthersInGroup(JointGroup.Name).SendAsync("messageOthers", DisplayName, message, Timestamp);
			await Clients.Caller.SendAsync("messageSelf", Timestamp, echo);
		}
	}

	// 加入或创建群组
	public async Task GroupEnterAsync(string? type, string? name, string? password) {
		if (string.IsNullOrWhiteSpace(name?.Trim()) || password is null) {
			// 参数错误
			await Clients.Caller.SendAsync("groupEnter", "failed", "参数错误！");
			return;
		}
		name = name.Trim();
		if (type == "create") {
			if (Cache.MemoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) && groups!.Exists(group => group.Name == name)) {
				// 群已存在
				await Clients.Caller.SendAsync("groupEnter", "eFailed");
				return;
			}
			var group = new Group(name, password) { MemberCount = 1 };
			if (groups is null) {
				_ = Cache.Set("ChatHub Groups", new List<Group> { group }, TimeSpan.FromHours(12));
			} else {
				lock (groups) {
					groups.Add(group);
				}
			}
			JointGroup = group;
			await Groups.AddToGroupAsync(Context.ConnectionId, name);
			await Clients.Caller.SendAsync("groupEnter", "cSuccess");
		} else if (type == "join") {
			if (!Cache.MemoryCache.TryGetValue<List<Group>>("ChatHub Groups", out var groups) || !groups!.Exists(group => group.Name == name)) {
				// 群不存在
				await Clients.Caller.SendAsync("groupEnter", "nFailed");
				return;
			}
			var group = groups!.First(group => group.Name == name);
			if (!group.VerifyPassword(password)) {
				// 密码错误
				await Clients.Caller.SendAsync("groupEnter", "pwdError");
				return;
			}
			JointGroup = group;
			lock (group) {
				group.MemberCount++;
			}
			await Groups.AddToGroupAsync(Context.ConnectionId, name);
			await Clients.Caller.SendAsync("groupEnter", "jSuccess");
			await Clients.OthersInGroup(name).SendAsync("messageServer", "Server", $"{DisplayName} 已加入群组。", Timestamp);
		} else {
			await Clients.Caller.SendAsync("groupEnter", "failed", $"加入群聊的方式 {type} 未定义！");
		}
	}

	// 离开群组
	public async Task GroupLeaveAsync() {
		if (JointGroup is null) {
			await Clients.Caller.SendAsync("groupLeave", "failed", "您尚未加入任何群组！");
			return;
		}

		lock (JointGroup) {
			if (--JointGroup.MemberCount == 0) {
				var groups = Cache.MemoryCache.Get<List<Group>>("ChatHub Groups");
				if (groups is null) {
					Clients.Caller.SendAsync("groupLeave", "failed", "服务端群组数据可能已被清除，请尝试重新进入聊天。");
					return;
				}
				lock (groups) {
					_ = groups.Remove(JointGroup);
				}
			}
		}

		var groupName = JointGroup.Name;
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
		JointGroup = null;

		await Clients.Caller.SendAsync("groupLeave", "success");
		await Clients.Group(groupName).SendAsync("messageServer", "Server", $"{DisplayName} 已离开群组。", Timestamp);
	}


	public override async Task OnDisconnectedAsync(Exception? exception) {
		if (JointGroup is not null) {
			lock (JointGroup) {
				if (--JointGroup.MemberCount == 0) {
					var groups = Cache.MemoryCache.Get<List<Group>>("ChatHub Groups")!;
					lock (groups) {
						_ = groups.Remove(JointGroup);
					}
				}
			}

			await Clients.OthersInGroup(JointGroup.Name).SendAsync("messageServer", "Server", $"{DisplayName} 已离开群组。", Timestamp);
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
		await Clients.Caller.SendAsync("notice", "Welcome", "连接成功！", "success");
		await base.OnConnectedAsync();
	}
}