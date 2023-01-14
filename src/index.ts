import * as signalR from "@microsoft/signalr";
import * as signalRProtocols from "@microsoft/signalr-protocol-msgpack";

import Swal, { SweetAlertIcon } from 'sweetalert2';
//import Swal from 'sweetalert2/src/sweetalert2.js';
import "./css/index.css";

const messagesDiv: HTMLDivElement = document.querySelector("#messages"),
	messageInput: HTMLInputElement = document.querySelector("#message"),
	userNameInput: HTMLInputElement = document.querySelector("#userName"),
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
	.withUrl("/chat")
	.withHubProtocol(new signalRProtocols.MessagePackHubProtocol())
	.build();

connection.on("messageReceived", (username: string, message: string) => {
	const m = document.createElement("div");

	m.innerHTML = `<div class="message-author">${username}</div><div>${message}</div>`;

	messagesDiv.appendChild(m);
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
		connection.send("newMessage", userNameInput.value, messageInput.value)
			.then(() => (messageInput.value = ""));
	}
});

sendMessageButton.addEventListener("click", () => connection.send("newMessage", userNameInput.value, messageInput.value)
	.then(() => (messageInput.value = "")));


joinGroupButton.addEventListener("click", () => {
	joiningGroupName = groupNameInput.value;
	connection.send("joinGroup", joiningGroupName, groupPasswordInput.value).catch((err) => {
		joiningGroupName = null;
		Swal.fire('加入失败', err, 'error');
	});
});

leaveGroupButton.addEventListener("click", () => connection.send("leaveGroup").catch((err) => Swal.fire('离开失败', err, 'error')));

createGroupButton.addEventListener("click", () => {
	joiningGroupName = groupNameInput.value;
	connection.send("createGroup", joiningGroupName, groupPasswordInput.value).catch((err) => {
		joiningGroupName = null;
		Swal.fire('创建失败', err, 'error');
	});
});