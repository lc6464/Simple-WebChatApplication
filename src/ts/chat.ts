import "../css/chat.css";

import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { MessagePackHubProtocol } from "@microsoft/signalr-protocol-msgpack";
import Swal, { SweetAlertIcon } from "sweetalert2/dist/sweetalert2.min.js";

import { fetchText, formatTime, randomUUID } from "./common";

const chatSection: HTMLElement = document.querySelector("#chat"),
	groupSection: HTMLElement = document.querySelector("#group"),
	messagesDiv: HTMLDivElement = document.querySelector("#messages"),
	joinGroupButton: HTMLButtonElement = document.querySelector("#join-group"),
	createGroupButton: HTMLButtonElement =
		document.querySelector("#create-group"),
	leaveGroupButton: HTMLButtonElement =
		document.querySelector("#leave-group");

let displayName: string = null,
	jointGroupName: string = null,
	joiningGroupName: string = null,
	isConnected = false;

// 消息上屏
function messageAddToScreen(
	sender: string,
	content: string,
	time: number | Date,
) {
	const container = document.createElement("div"),
		senderSpan = document.createElement("span"),
		messageTimeSpan = document.createElement("span"),
		contentDiv = document.createElement("div");

	container.className = "content";

	senderSpan.className = "sender";
	messageTimeSpan.className = "time";
	contentDiv.className = "content-content";

	senderSpan.innerText = sender;
	messageTimeSpan.innerText = formatTime(
		typeof time === "number" ? new Date(time) : time,
	);
	contentDiv.innerText = content;

	container.appendChild(senderSpan);
	container.appendChild(messageTimeSpan);
	container.appendChild(contentDiv);

	messagesDiv.appendChild(container);

	//messagesDiv.scrollTop = messagesDiv.scrollHeight;

	return container;
}

function addEventListeners(connection: HubConnection) {
	const messageInput: HTMLInputElement =
		document.querySelector("#content-input"),
		sendMessageButton: HTMLButtonElement =
			document.querySelector("#send-content"),
		groupNameInput: HTMLInputElement =
			document.querySelector("#group-name"),
		groupPasswordInput: HTMLInputElement =
			document.querySelector("#group-password");

	// 发消息
	sendMessageButton.addEventListener("click", () => {
		const value = messageInput.value;
		if (value.trim() !== "") {
			messageInput.value = "";
			const uuid = `${randomUUID()}-${Date.now()}`,
				container = messageAddToScreen(displayName, value, new Date());
			container.classList.add("self");
			container.classList.add("sending");
			container.dataset.echo = uuid;
			connection.send("messageAsync", value, uuid).catch((e) => {
				console.error(`消息 ${uuid} 发送失败：`, e);
				if (container.classList.contains("sending")) {
					container.classList.remove("sending");
					container.classList.add("send-failed");
				} else {
					console.warn(
						`消息 ${uuid} 发送异常后却接收到响应，暂且视为发送成功。`,
					);
				}
			});
		}
	});

	messageInput.addEventListener("keydown", (e: KeyboardEvent) => {
		if (e.key === "Enter") {
			sendMessageButton.click();
		}
	});

	// 加群
	joinGroupButton.addEventListener("click", () => {
		const value = groupNameInput.value;
		if (value.trim() !== "") {
			joinGroupButton.disabled = true;
			createGroupButton.disabled = true;
			joiningGroupName = value;
			connection
				.send(
					"groupEnterAsync",
					"join",
					joiningGroupName,
					groupPasswordInput.value,
				)
				.catch((err) => {
					joiningGroupName = null;
					console.error("加入群聊失败：", err);
					Swal.fire("加入失败", err, "error");
				});
		}
	});

	groupPasswordInput.addEventListener("keydown", (e: KeyboardEvent) => {
		if (e.key === "Enter") {
			joinGroupButton.click();
		}
	});

	// 建群
	createGroupButton.addEventListener("click", () => {
		const value = groupNameInput.value;
		if (value.trim() !== "") {
			joinGroupButton.disabled = true;
			createGroupButton.disabled = true;
			joiningGroupName = value;
			connection
				.send(
					"groupEnterAsync",
					"create",
					joiningGroupName,
					groupPasswordInput.value,
				)
				.catch((err) => {
					joiningGroupName = null;
					console.error("创建群聊失败：", err);
					Swal.fire("创建失败", err, "error");
				});
		}
	});

	// 退群
	leaveGroupButton.addEventListener("click", () =>
		connection.send("groupLeaveAsync").catch((err) => {
			console.error("离开群聊失败：", err);
			Swal.fire("离开失败", err, "error");
		}),
	);
}

function connectSignalR() {
	const jointGroupNameSpan: HTMLSpanElement =
		document.querySelector("#joint-group-name");

	const connection = new HubConnectionBuilder()
		.withUrl("/chathub")
		.withHubProtocol(new MessagePackHubProtocol())
		.build();

	// 通知进入群聊状态相关
	connection.on("groupEnter", (type: string) => {
		createGroupButton.disabled = false;
		joinGroupButton.disabled = false;
		let title: string = null,
			error: string = null;
		switch (type) {
			case "cSuccess":
			case "jSuccess":
				jointGroupName = joiningGroupName;
				joiningGroupName = null;
				jointGroupNameSpan.innerText = jointGroupName;
				groupSection.classList.remove("ready");
				chatSection.classList.add("ready");
				title = type.startsWith("c") ? "创建" : "加入";
				Swal.fire(
					`${title}成功`,
					`您已成功${title}群聊 ${jointGroupName}。`,
					"success",
				);
				break;
			case "pwdError":
				joiningGroupName = null;
				Swal.fire("密码错误", "您输入的密码错误！", "error");
				break;
			case "eFailed":
			case "nFailed":
				joiningGroupName = null;
				[title, error] = type.startsWith("e")
					? ["创建", "已存在"]
					: ["加入", "不存在"];
				Swal.fire(
					`${title}失败`,
					`群聊 ${joiningGroupName} ${error}！`,
					"error",
				);
				break;
			default:
				console.warn(`未知的 groupEnter 类型：${type}`);
		}
	});
    //通知群组退出状态
	connection.on("groupLeave", (type: string, message?: string) => {
		leaveGroupButton.disabled = false;
		if (type === "success") {
			jointGroupNameSpan.innerText = "";
			chatSection.classList.remove("ready");
			groupSection.classList.add("ready");
			messagesDiv.innerHTML = "";
			Swal.fire(
				"离开成功",
				`您已离开群聊 ${jointGroupName}。`,
				"success",
			);
			jointGroupName = null;
			return;
		}
		Swal.fire(
			"离开失败",
			`离开群聊 ${jointGroupName} 失败！${message}`,
			"error",
		);
	});

	// 接收到其他人的消息
	connection.on(
		"messageOthers",
		(sender: string, content: string, time: number) => {
			messageAddToScreen(sender, content, time);
		},
	);

	// 接收到来自服务器的消息
	connection.on(
		"messageServer",
		(sender: string, content: string, time: number) => {
			messageAddToScreen(sender, content, time).classList.add("server");
		},
	);

	// 接收到自己的消息的回调
	connection.on("messageSelf", (time: number, echo: string) => {
		document.querySelectorAll<HTMLDivElement>(".sending").forEach((e) => {
			if (e.dataset.echo === echo) {
				e.classList.remove("sending");
				e.querySelector<HTMLSpanElement>(".time").innerText =
					formatTime(new Date(time));
			}
		});
	});

	// 服务器通知
	connection.on(
		"notice",
		(
			titleOrParams: string | object,
			message?: string,
			icon?: SweetAlertIcon,
		) => {
			if (typeof titleOrParams === "string") {
				Swal.fire(titleOrParams, message, icon);
			} else {
				Swal.fire(titleOrParams);
			}
		},
	);

	// 断开连接
	connection.onclose((e) => {
		console.error("与服务器的连接断开：", e);
		isConnected = false;
		Swal.fire({
			title: "连接断开",
			text: "与服务器的连接已断开！请刷新页面重试。详细信息请见控制台。",
			icon: "error",
		});
	});

	// 建立连接
	connection.start().catch((e) => {
		console.error("连接服务器失败：", e);
		if (isConnected) {
			Swal.fire({
				title: "连接断开",
				text: "与服务器的连接已断开！请刷新页面重试。详细信息请见控制台。",
				icon: "error",
			});
		} else {
			Swal.fire({
				title: "连接失败",
				text: "连接到服务器时发生错误！详细信息请见控制台。",
				icon: "error",
				footer: '<a href="contact" title="联系站长">点此联系站长</a>',
			});
		}
	});

	addEventListeners(connection);
}

(async function () {
	Swal.fire({
		title: "正在连接",
		html: "正在连接，请稍候。",
		didOpen: () => {
			Swal.showLoading();
		},
	});
	const login = await fetchText("api/login");
	if (!login.success) {
		Swal.fire({
			title: "检查登录状态失败",
			text: login.message,
			icon: "error",
			footer: '<a href="contact" title="联系站长">点此联系站长</a>',
		});
		// @ts-expect-error result is parsed data
	} else if (!login.result.success) {
		Swal.fire("未登录", "您尚未登录！即将进入登录页面。", "warning").then(
			() => (location.href = "login.html"),
		);
	} else {
		console.log("已登录。");
		// @ts-expect-error result is parsed data
		displayName = login.result.displayName;
		groupSection.classList.add("ready");
		connectSignalR();
	}
})().catch((e) => {
	console.error("入口点函数发生异常：", e);
});
