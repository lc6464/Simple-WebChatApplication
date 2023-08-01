import "../css/login.css";

import Swal from "sweetalert2/dist/sweetalert2.min.js";

import { fetchText, copyText } from "./common";

const form: HTMLFormElement = document.querySelector("form"),
	registerButton: HTMLButtonElement = document.querySelector("#register");

form.addEventListener('submit', e => e.preventDefault());

registerButton.addEventListener("click", () => {
	(async () => {
		const { success, result, message } = await fetchText("api/register", {
			body: new FormData(form),
			method: "post",
		});
		// @ts-expect-error result is parsed data
		if (success && result.success && result.data !== null) {
			// @ts-expect-error result is parsed data
			if (await copyText(result.data)) {
				Swal.fire({
					title: "复制成功",
					text: "已将密钥到剪贴板，请发送给管理员。",
					icon: "success"
				});
			} else {
				Swal.fire({
					title: "复制失败",
					// @ts-expect-error result is parsed data
					text: `${result.data}`,
					icon: "error",
					footer: '请手动复制密钥后发送给管理员。'
				});
			}
		} else {
			Swal.fire("注册失败", message, "warning");
		}
	})().then(() => { console.log('function register has been executed.'); });
});