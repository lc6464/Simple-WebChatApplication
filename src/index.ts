import * as signalR from "@microsoft/signalr";
import * as signalRProtocols from "@microsoft/signalr-protocol-msgpack";
import "./css/main.css";

const divMessages: HTMLDivElement = document.querySelector("#divMessages"),
	tbMessage: HTMLInputElement = document.querySelector("#tbMessage"),
	tbUsername: HTMLInputElement = document.querySelector("#tbUsername"),
	btnSend: HTMLButtonElement = document.querySelector("#btnSend");


const connection = new signalR.HubConnectionBuilder()
	.withUrl("/hub")
	.withHubProtocol(new signalRProtocols.MessagePackHubProtocol())
	.build();

connection.on("messageReceived", (username: string, message: string) => {
	const m = document.createElement("div");

	m.innerHTML = `<div class="message-author">${username}</div><div>${message}</div>`;

	divMessages.appendChild(m);
	divMessages.scrollTop = divMessages.scrollHeight;
});

connection.start().catch((err) => document.write(err));

tbMessage.addEventListener("keydown", (e: KeyboardEvent) => {
	if (e.key === "Enter") {
		send();
	}
});

btnSend.addEventListener("click", send);

function send() {
	connection.send("newMessage", tbUsername.value, tbMessage.value)
		.then(() => (tbMessage.value = ""));
}