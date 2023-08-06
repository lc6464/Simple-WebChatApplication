import "../css/login.css";

import Swal from "sweetalert2/dist/sweetalert2.min.js";

import { fetchText, copyText, AccountCheckingTools } from "./common";

const form: HTMLFormElement = document.querySelector("form"),
	registerButton: HTMLButtonElement = document.querySelector("#register"),
	dataTextArea: HTMLTextAreaElement = document.querySelector("#data"),
	dataContainer: HTMLDivElement = document.querySelector("#data-container");

form.addEventListener("submit", (e) => e.preventDefault());

registerButton.addEventListener("click", () => {
	(async () => {
		const { success, result, message } = await fetchText("api/register", {
			body: new FormData(form),
			method: "post",
		});
		
		if (success) {
			// @ts-expect-error result is parsed data
			const { success, data, message }: { success: boolean; data: string; message: string } = result;
			if (success) {
				dataTextArea.value = data;
				dataContainer.classList.add("ready");
				Swal.fire("注册成功", "已成功注册！请将展示的数据复制后发送给网站管理员，待管理员审核通过后即可登录。", "success");
			} else {
				Swal.fire("注册失败", message, "error");
			}
		} else {
			Swal.fire("注册失败", message, "warning");
		}
	})().catch((e) => {
		console.log("注册函数发生异常：", e);
	});
});

document.querySelector("#copy").addEventListener("click", () => {
	copyText(dataTextArea.value)
		.then((e) => {
			if (e) {
				Swal.fire({
					title: "复制成功",
					text: "已将密钥到剪贴板，请发送给管理员。",
					icon: "success",
				});
			} else {
				Swal.fire({
					title: "复制失败",
					text: "请手动复制密钥后发送给管理员。",
					icon: "error",
				});
			}
		})
		.catch((e) => {
			console.log("复制函数发生异常：", e);
		});
});
