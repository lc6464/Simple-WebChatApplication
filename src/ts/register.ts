import "../css/login.css";

import Swal from "sweetalert2/dist/sweetalert2.min.js";

import { fetchText, copyText, AccountCheckingTools } from "./common";

const form: HTMLFormElement = document.querySelector("form"),
	registerButton: HTMLButtonElement = document.querySelector("#register"),
	dataTextArea: HTMLTextAreaElement = document.querySelector("#data"),
	dataContainer: HTMLDivElement = document.querySelector("#data-container"),
	accountNameInput: HTMLInputElement = document.querySelector("#account"),
	passwordInput: HTMLInputElement = document.querySelector("#password"),
	repeatPasswordInput: HTMLInputElement = document.querySelector("#repeat-password");

form.addEventListener("submit", (e) => e.preventDefault());

registerButton.addEventListener("click", () => {
	// 表单检查
	const {result, type, message} = formCheck();
	if (!result) {
		Swal.fire("注册失败", message, "warning");
		return;
	}
	// 提交表单
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

// 表单检查
function formCheck() {
	if (AccountCheckingTools.isEmptyOrWhiteSpace(accountNameInput.value)) {
		return {result: false, type: "account", message: "用户名不能为空或空格"};
	}
	if (AccountCheckingTools.isEmptyOrWhiteSpace(passwordInput.value)) {
		return {result: false, type: "password", message: "密码不能为空或空格"};
	}
	if (passwordInput.value === repeatPasswordInput.value) {
		return {result: false, type: "repeat-password", message: "请确认两次输入的密码是否一致"};
	}
	if (AccountCheckingTools.isNameUnable(accountNameInput.value)) {
		return {result: false, type: "account", message:"用户名必须在4~32个字符之间，并且仅可包含大小写字母，数字和特殊字符(+-_$)"};
	}
	const {result, message} = AccountCheckingTools.isPasswordComplicated(passwordInput.value);
	if (!result) {
		return {result: false, type: "password", message: message};
	}
	return {result: true, type: "success", message:""};
}
