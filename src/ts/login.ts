import "../css/login.css";

import Swal from "sweetalert2/dist/sweetalert2.min.js";

import {AccountCheckingTools, fetchText} from "./common";

const form: HTMLFormElement = document.querySelector("form"),
	loginButton: HTMLButtonElement = document.querySelector("#login"),
	resetPasswordAnchor: HTMLAnchorElement =
		document.querySelector("#reset-password"),
	accountInput: HTMLInputElement = document.querySelector("#account"),
	passwordInput: HTMLInputElement = document.querySelector("#password");

form.addEventListener("submit", (e) => e.preventDefault());
resetPasswordAnchor.addEventListener("click", (e) => {
	e.preventDefault();
	Swal.fire({
		title: "重置密码",
		text: "请联系管理员重置密码。",
		icon: "info",
		footer: '<a href="contact" title="联系站长">点此联系站长</a>',
	});
});

loginButton.addEventListener("click", () => {
	const {result, message} = formCheck();
	if (!result) {
		Swal.fire("登陆失败", message, "warning")
		return;
	}
	
	(async () => {
		const { success, result, message } = await fetchText("api/login", {
			body: new FormData(form),
			method: "post",
		});
		if (success) {
			// @ts-expect-error result is parsed data
			if (result.success) {
				Swal.fire("登录成功", "即将跳转到主页。", "success").then(
					() => (location.href = "index.html"),
				);
			} else {
				// @ts-expect-error result is parsed data
				Swal.fire("登录失败", result.message, "error");
			}
		} else {
			Swal.fire("登录失败", message, "warning");
		}
	})().catch((e) => {
		console.log("登录函数发生异常：", e);
	});
});

// 表单检查
function formCheck() {
	if (AccountCheckingTools.isNullOrWhiteSpace(accountInput.value) || 
		AccountCheckingTools.isNullOrWhiteSpace(passwordInput.value)) {
		return {
			result: false,
			message: "用户名或密码为空！",
		};
	}
	const {result, message} = AccountCheckingTools.isPasswordComplicated(passwordInput.value);
	if (AccountCheckingTools.isNameUnable(accountInput.value) || !result) {
		return {
			result: false,
			message:
				"用户名或密码错误！",
		};
	}
	return {result: true, message: ''};
}
