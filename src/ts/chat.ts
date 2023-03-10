import * as signalR from "@microsoft/signalr";
import * as signalRProtocols from "@microsoft/signalr-protocol-msgpack";

import Swal, { SweetAlertIcon } from 'sweetalert2';
import "../css/chat.css";

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
			Swal.fire('????????????', message, icon);
			break;
		case 'joinFailed':
			joiningGroupName = null;
			Swal.fire('????????????', message, icon);
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
			Swal.fire('????????????', message, icon);
			break;
		case 'leaveFailed':
			Swal.fire('????????????', message, icon);
			break;
		case 'sendFailed':
			Swal.fire('????????????', message, icon);
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
			Swal.fire('????????????', message, icon);
			break;
		case 'createFailed':
			joiningGroupName = null;
			Swal.fire('????????????', message, icon);
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
		Swal.fire('????????????', err, 'error');
	});
});

leaveGroupButton.addEventListener("click", () => connection.send("leaveGroupAsync").catch((err) => Swal.fire('????????????', err, 'error')));

createGroupButton.addEventListener("click", () => {
	joiningGroupName = groupNameInput.value;
	connection.send("createGroupAsync", joiningGroupName, groupPasswordInput.value).catch((err) => {
		joiningGroupName = null;
		Swal.fire('????????????', err, 'error');
	});
});

groupPasswordInput.addEventListener("keydown", (e: KeyboardEvent) => {
	if (e.key === "Enter") {
		joinGroupButton.click();
	}
});