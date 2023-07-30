import * as signalR from "@microsoft/signalr";
import * as signalRProtocols from "@microsoft/signalr-protocol-msgpack";

import Swal, { SweetAlertIcon } from 'sweetalert2/dist/sweetalert2.min.js';
import "../css/chat.css";
import {fetchData} from "./common";
import { group } from "console";

/*

REST API 检查是否登录
SignalR 连接
加群
添加两个 .ready
接收消息
发送消息，清除 input，立即上屏，接收 echo 更新时间并移除 .sending，视情况添加 .send-failed
离开群，移除 .ready，清空 .messages


*/


const chatSection: HTMLElement = document.querySelector("#chat"),
	groupSection: HTMLElement = document.querySelector("#group");


async function checkLogin() {
	return fetchData('api/login', {
		method: 'get'
	});
}

async function main() {
	Swal.fire({
		title: '正在连接',
		html: '正在连接，请稍候。',
		didOpen: () => {
			Swal.showLoading()
		}
	});
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
		console.log('已登录。');
		groupSection.classList.add('ready');
		connectSignalR();
	}
}


function connectSignalR() {
	const messagesDiv: HTMLDivElement = document.querySelector("#messages"),
		messageInput: HTMLInputElement = document.querySelector("#message-input"),
		sendMessageButton: HTMLButtonElement = document.querySelector("#send-message"),
		groupNameInput: HTMLInputElement = document.querySelector("#group-name"),
		groupPasswordInput: HTMLInputElement = document.querySelector("#group-password"),
		joinGroupButton: HTMLButtonElement = document.querySelector("#join-group"),
		createGroupButton: HTMLButtonElement = document.querySelector("#create-group"),
		leaveGroupButton: HTMLButtonElement = document.querySelector("#leave-group"),
		jointGroupNameSpan: HTMLSpanElement = document.querySelector("#joint-group-name");
		


	let jointGroupName: string = null,
		joiningGroupName: string = null,
		isConnected: boolean = false;


	const connection = new signalR.HubConnectionBuilder()
		.withUrl("/chathub")
		.withHubProtocol(new signalRProtocols.MessagePackHubProtocol())
		.build();

	connection.on("group", (result: string, type: string, message: string, echo: string) => {
		if (result === 'success') {
			if (type === 'leave') {
				jointGroupName = null;
				jointGroupNameSpan.innerText = '';
				chatSection.classList.remove('ready');
				groupSection.classList.add('ready');
				Swal.fire('离开成功', message, 'success');
				return;
			}
			jointGroupName = joiningGroupName;
			joiningGroupName = null;
			jointGroupNameSpan.innerText = jointGroupName;
			groupSection.classList.remove('ready');
			chatSection.classList.add('ready');
			Swal.fire('操作成功', message, 'success');
			return;
		}
		/* 过时代码，但是可以作为参考
		switch (result) {
			case 'joinFailed':
				joiningGroupName = null;
				Swal.fire('加入失败', message, icon);
				break;
			case 'leaveFailed':
				Swal.fire('离开失败', message, icon);
				break;
			case 'sendFailed':
				Swal.fire('发送失败', message, icon);
				break;
				break;
			case 'createFailed':
				joiningGroupName = null;
				Swal.fire('创建失败', message, icon);
				break;
			default:
				Swal.fire(result, message, icon);
		}
		*/
	});


	connection.on("message", (sender: string, message: string, time: string, echo: string) => {
		if (echo === '') {
			const container = document.createElement('div'),
				userNameSpan = document.createElement('span'),
				userNameDiv = document.createElement('div'),
				messageTimeSpan = document.createElement('span'),
				messageDiv = document.createElement('div');

			container.className = 'message';
			
			userNameDiv.innerText = 'username';
			messageDiv.innerText = `${time} ${message}`;

			container.appendChild(userNameDiv);
			container.appendChild(messageDiv);

			messagesDiv.appendChild(container);
			//messagesDiv.scrollTop = messagesDiv.scrollHeight;
		}
	});



	connection.start().catch(e => {
		console.error(e);
		if (isConnected) {
			Swal.fire({
				title: '连接断开',
				text: '与服务器的连接已断开！请刷新页面重试。详细信息请见控制台。',
				icon: 'error'
			});
		} else {
			Swal.fire({
				title: '连接失败',
				text: '连接到服务器时发生错误！详细信息请见控制台。',
				icon: 'error',
				footer: '<a href="contact" title="联系站长">点此联系站长</a>'
			});
		}
	});

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
<div class="message self sending" data-echo="xxx"><span class="sender">测试人员 (Tester)</span><span class="time">2023-7-18 5:55:05</span><div class="message-content"></div></div>
<div class="message self send-failed"><span class="sender">测试人员 (Tester)</span><span class="time">2023-7-18 5:55:05</span><div class="message-content"></div></div>
*/