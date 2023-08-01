import "../css/chat.css";

import * as signalR from "@microsoft/signalr";
import * as signalRProtocols from "@microsoft/signalr-protocol-msgpack";
import { Swal, SweetAlertIcon } from 'sweetalert2/dist/sweetalert2.min.js';

import { fetchText, formatTime, randomUUID } from "./common";

/*

REST API 检查是否登录
SignalR 连接
加群
添加两个 .ready
接收消息
发送消息，清除 input，立即上屏，接收 echo 更新时间并移除 .sending，视情况添加 .send-failed
离开群，移除 .ready，清空 .messages


*/


// 消息上屏
function messageAddToScreen(sender: string, content: string, time: number | Date) {
	const container = document.createElement('div'),
		senderSpan = document.createElement('span'),
		messageTimeSpan = document.createElement('span'),
		contentDiv = document.createElement('div');

	container.className = 'content';

	senderSpan.className = 'sender';
	messageTimeSpan.className = 'time';
	contentDiv.className = 'content-content';

	senderSpan.innerText = sender;
	messageTimeSpan.innerText = formatTime(typeof time === 'number' ? new Date(time) : time);
	contentDiv.innerText = content;

	container.appendChild(senderSpan);
	container.appendChild(messageTimeSpan);
	container.appendChild(contentDiv);

	messagesDiv.appendChild(container);

	//messagesDiv.scrollTop = messagesDiv.scrollHeight;

	return container;
}



const chatSection: HTMLElement = document.querySelector("#chat"),
	groupSection: HTMLElement = document.querySelector("#group"),
	messagesDiv: HTMLDivElement = document.querySelector("#messages");

let displayName: string = null,
	jointGroupName: string = null,
	joiningGroupName: string = null,
	isConnected: boolean = false;


async function main() {
	Swal.fire({
		title: '正在连接',
		html: '正在连接，请稍候。',
		didOpen: () => {
			Swal.showLoading()
		}
	});
	const login = await fetchText('api/login');
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
		displayName = login.result.displayName;
		groupSection.classList.add('ready');
		connectSignalR();
	}
}


function connectSignalR() {
	const jointGroupNameSpan: HTMLSpanElement = document.querySelector("#joint-group-name");

	const connection = new signalR.HubConnectionBuilder()
		.withUrl("/chathub")
		.withHubProtocol(new signalRProtocols.MessagePackHubProtocol())
		.build();

	// 群聊相关 未完成
	connection.on('group', (result: string, type: string, message: string, echo: string) => {
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
				Swal.fire('加入失败', content, icon);
				break;
			case 'leaveFailed':
				Swal.fire('离开失败', content, icon);
				break;
			case 'sendFailed':
				Swal.fire('发送失败', content, icon);
				break;
				break;
			case 'createFailed':
				joiningGroupName = null;
				Swal.fire('创建失败', content, icon);
				break;
			default:
				Swal.fire(result, content, icon);
		}
		*/
	});



	// 接收到其他人的消息
	connection.on('messageOthers', (sender: string, content: string, time: number) => {
		messageAddToScreen(sender, content, time);
	});

	// 接收到来自服务器的消息
	connection.on('messageServer', (sender: string, content: string, time: number) => {
		messageAddToScreen(sender, content, time).classList.add('server');
	});

	// 接收到自己的消息的回调
	connection.on('messageSelf', (time: number, echo: string) => {
		document.querySelectorAll<HTMLDivElement>('.sending').forEach(e => {
			if (e.dataset.echo === echo) {
				e.classList.remove('sending');
				e.querySelector<HTMLSpanElement>('.time').innerText = formatTime(new Date(time));
			}
		});
	});

	// 服务器通知
	connection.on('notice', (titleOrParams: string | object, message?: string, icon?: SweetAlertIcon) => {
		if (typeof titleOrParams === 'string') {
			Swal.fire(titleOrParams, message, icon);
		} else {
			Swal.fire(titleOrParams);
		}
	});


	connection.onclose(e => {
		console.error('与服务器的连接断开：', e);
		isConnected = false;
		Swal.fire({
			title: '连接断开',
			text: '与服务器的连接已断开！请刷新页面重试。详细信息请见控制台。',
			icon: 'error'
		});
	});

	// 建立连接
	connection.start().catch(e => {
		console.error('连接服务器失败：', e);
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

	addEventListener(connection);
}


function addEventListener(connection: signalR.HubConnection) {
	const messageInput: HTMLInputElement = document.querySelector("#content-input"),
		sendMessageButton: HTMLButtonElement = document.querySelector("#send-content"),
		groupNameInput: HTMLInputElement = document.querySelector("#group-name"),
		groupPasswordInput: HTMLInputElement = document.querySelector("#group-password"),
		joinGroupButton: HTMLButtonElement = document.querySelector("#join-group"),
		createGroupButton: HTMLButtonElement = document.querySelector("#create-group"),
		leaveGroupButton: HTMLButtonElement = document.querySelector("#leave-group");


	// 发消息
	sendMessageButton.addEventListener('click', () => {
		const value = messageInput.value.trim();
		if (value !== '') {
			messageInput.value = '';
			const uuid = `${randomUUID()}-${Date.now()}`,
				container = messageAddToScreen(displayName, value, new Date());
			container.classList.add('self');
			container.classList.add('sending');
			container.dataset.echo = uuid;
			connection.send('messageAsync', value, uuid).catch(e => {
				console.error(`消息 ${uuid} 发送失败：`, e);
				if (container.classList.contains('sending')) {
					container.classList.remove('sending');
					container.classList.add('send-failed');
				} else {
					console.warn(`消息 ${uuid} 发送异常后却接收到响应，暂且视为发送成功。`);
				}
			});
		}
	});

	messageInput.addEventListener('keydown', (e: KeyboardEvent) => {
		if (e.key === 'Enter') {
			sendMessageButton.click();
		}
	});

	// 加群 似乎未完成
	joinGroupButton.addEventListener('click', () => {
		joiningGroupName = groupNameInput.value;
		connection.send('joinGroupAsync', joiningGroupName, groupPasswordInput.value).catch((err) => {
			joiningGroupName = null;
			Swal.fire('加入失败', err, 'error');
		});
	});

	groupPasswordInput.addEventListener('keydown', (e: KeyboardEvent) => {
		if (e.key === 'Enter') {
			joinGroupButton.click();
		}
	});

	// 建群 似乎未完成
	createGroupButton.addEventListener('click', () => {
		joiningGroupName = groupNameInput.value;
		connection.send('createGroupAsync', joiningGroupName, groupPasswordInput.value).catch((err) => {
			joiningGroupName = null;
			Swal.fire('创建失败', err, 'error');
		});
	});

	// 退群 似乎未完成
	leaveGroupButton.addEventListener('click', () => connection.send('leaveGroupAsync').catch((err) => Swal.fire('离开失败', err, 'error')));
}


main();