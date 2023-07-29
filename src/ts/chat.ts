import * as signalR from "@microsoft/signalr";
import * as signalRProtocols from "@microsoft/signalr-protocol-msgpack";

import Swal, { SweetAlertIcon } from 'sweetalert2/dist/sweetalert2.min.js';
import "../css/chat.css";

/*

REST API 检查是否登录
SignalR 连接
加群
添加两个 .ready
接收消息
发送消息，清除 input，立即上屏，接收 echo 更新时间并移除 .sending，视情况添加 .send-failed
离开群，移除 .ready，清空 .messages


*/


async function checkLogin() {
	let message = '';
	try {
		const response = await fetch('api/login', {
			method: 'get'
		});
		if (response.ok) {
			try {
				return { success: true, result: await response.json(), message };
			} catch (e) {
				message = '在解析 JSON 过程中发生异常，详细信息请见控制台！';
				console.error('在解析 JSON 过程中发生异常：', e);
			}
		} else {
			message = '在 Fetch 过程中接收到了不成功的状态码，详细信息请见控制台！';
			console.error('在 Fetch 过程中接收到了不成功的状态码，响应对象：', response);
		}
	} catch (e) {
		message = '在 Fetch 过程中发生异常，详细信息请见控制台！';
		console.error('在 Fetch 过程中发生异常：', e);
	}
	return { success: false, result: null, message };
}


async function main() {
	const login = await checkLogin();
	if (!login.success) {
		Swal.fire({
			title: '检查登录状态失败',
			text: login.message,
			icon: 'error',
			footer: '<a href="contact" title="联系站长">点此联系站长</a>'
		});
	} else if (!login.result.success) {
		Swal.fire('未登录', '您尚未登录！即将进入登录页面。', 'warning').then(() => location.href = 'login.html');
	} else {
		connectSignalR();
	}
}


function connectSignalR() {
	const messagesDiv: HTMLDivElement = document.querySelector("#messages"),
		messageInput: HTMLInputElement = document.querySelector("#message"),
		sendMessageButton: HTMLButtonElement = document.querySelector("#sendMessage"),
		groupNameInput: HTMLInputElement = document.querySelector("#groupName"),
		jointGroupNameSpan: HTMLSpanElement = document.querySelector("#jointGroupName"),
		groupPasswordInput: HTMLInputElement = document.querySelector("#groupPassword"),
		groupPasswordLabel: HTMLLabelElement = document.querySelector("#groupPasswordLabel"),
		joinGroupButton: HTMLButtonElement = document.querySelector("#joinGroup"),
		createGroupButton: HTMLButtonElement = document.querySelector("#createGroup"),
		leaveGroupButton: HTMLButtonElement = document.querySelector("#leaveGroup");

	let jointGroupName: string = null,
		joiningGroupName: string = null;


	const connection = new signalR.HubConnectionBuilder()
		.withUrl("/chathub")
		.withHubProtocol(new signalRProtocols.MessagePackHubProtocol())
		.build();

	connection.on("messageReceived", (username: string, message: string, time: string) => {
		const container = document.createElement("div"),
			userNameDiv = document.createElement("div"),
			messageDiv = document.createElement("div");

		userNameDiv.className = "messageUserName";
		messageDiv.className = "messageText";

		userNameDiv.innerText = username;
		messageDiv.innerText = `${time} ${message}`;

		container.appendChild(userNameDiv);
		container.appendChild(messageDiv);

		messagesDiv.appendChild(container);
		messagesDiv.scrollTop = messagesDiv.scrollHeight;
	});

	connection.on("groupResult", (result: string, icon: SweetAlertIcon, message: string) => {
		switch (result) {
			case 'joinSuccess':
				jointGroupName = joiningGroupName;
				joiningGroupName = null;
				jointGroupNameSpan.innerText = jointGroupName;
				jointGroupNameSpan.style.display = 'inline';
				joinGroupButton.style.display = 'none';
				createGroupButton.style.display = 'none';
				leaveGroupButton.style.display = 'inline';
				groupNameInput.style.display = 'none';
				groupPasswordLabel.style.display = 'none';
				Swal.fire('加入成功', message, icon);
				break;
			case 'joinFailed':
				joiningGroupName = null;
				Swal.fire('加入失败', message, icon);
				break;
			case 'leaveSuccess':
				jointGroupName = null;
				jointGroupNameSpan.innerText = '';
				jointGroupNameSpan.style.display = 'none';
				joinGroupButton.style.display = 'inline';
				createGroupButton.style.display = 'inline';
				leaveGroupButton.style.display = 'none';
				groupNameInput.style.display = 'inline';
				groupPasswordLabel.style.display = 'inline';
				Swal.fire('离开成功', message, icon);
				break;
			case 'leaveFailed':
				Swal.fire('离开失败', message, icon);
				break;
			case 'sendFailed':
				Swal.fire('发送失败', message, icon);
				break;
			case 'createSuccess':
				jointGroupName = joiningGroupName;
				joiningGroupName = null;
				jointGroupNameSpan.innerText = jointGroupName;
				jointGroupNameSpan.style.display = 'inline';
				joinGroupButton.style.display = 'none';
				createGroupButton.style.display = 'none';
				leaveGroupButton.style.display = 'inline';
				groupNameInput.style.display = 'none';
				groupPasswordLabel.style.display = 'none';
				Swal.fire('创建成功', message, icon);
				break;
			case 'createFailed':
				joiningGroupName = null;
				Swal.fire('创建失败', message, icon);
				break;
			default:
				Swal.fire(result, message, icon);
		}
	});

	connection.start().catch((err) => document.write(err));

	messageInput.addEventListener("keydown", (e: KeyboardEvent) => {
		if (e.key === "Enter") {
			sendMessageButton.click();
		}
	});

	sendMessageButton.addEventListener("click", () => {
		if (messageInput.value.trim() !== '') {
			connection.send("sendMessageAsync", messageInput.value).then(() => (messageInput.value = ""));
		}
	});


	joinGroupButton.addEventListener("click", () => {
		joiningGroupName = groupNameInput.value;
		connection.send("joinGroupAsync", joiningGroupName, groupPasswordInput.value).catch((err) => {
			joiningGroupName = null;
			Swal.fire('加入失败', err, 'error');
		});
	});

	leaveGroupButton.addEventListener("click", () => connection.send("leaveGroupAsync").catch((err) => Swal.fire('离开失败', err, 'error')));

	createGroupButton.addEventListener("click", () => {
		joiningGroupName = groupNameInput.value;
		connection.send("createGroupAsync", joiningGroupName, groupPasswordInput.value).catch((err) => {
			joiningGroupName = null;
			Swal.fire('创建失败', err, 'error');
		});
	});

	groupPasswordInput.addEventListener("keydown", (e: KeyboardEvent) => {
		if (e.key === "Enter") {
			joinGroupButton.click();
		}
	});
}


main();



/*
<section id="chat" class="ready">
<section id="group" class="ready">
<div class="message"><span class="sender">ABCD (aaa1564613)</span><span class="time">2023-7-18 5:55:05</span><div class="message-content"></div></div>
<div class="message server"><span class="sender">Server</span><span class="time">2023-7-18 5:55:05</span><div class="message-content"></div></div>
<div class="message self"><span class="sender">测试人员 (Tester)</span><span class="time">2023-7-18 5:55:05</span><div class="message-content"></div></div>
<div class="message self sending"><span class="sender">测试人员 (Tester)</span><span class="time">2023-7-18 5:55:05</span><div class="message-content"></div></div>
<div class="message self send-failed"><span class="sender">测试人员 (Tester)</span><span class="time">2023-7-18 5:55:05</span><div class="message-content"></div></div>
*/