import "../css/login.css";

import Swal from "sweetalert2/dist/sweetalert2.min.js";

import { fetchText, copyText } from "./common";

const form: HTMLFormElement = document.querySelector("form"),
	registerButton: HTMLButtonElement = document.querySelector("#register");

document.querySelector("html").addEventListener("click", (e) => {
	const target = e.target as HTMLElement,
		isLinks = target instanceof HTMLAnchorElement,
		isNavLogo =
			(target instanceof HTMLSpanElement ||
				target instanceof HTMLImageElement) &&
			target.id.startsWith("navLogo");
	console.log(target);
	if (isLinks || isNavLogo) {
		return;
	}
	e.preventDefault();
});
document.querySelector("form").addEventListener("click", () => {
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
					text: "已将密钥到剪贴板，请发送给管理员",
					icon: "success",
				});
			} else {
				Swal.fire({
					title: "复制失败",
					// @ts-expect-error result is parsed data
					text: `请手动复制密钥后发送给管理员：\n${result.data}`,
					icon: "error",
				});
			}
		} else {
			Swal.fire("注册失败", message, "warning");
		}
	})();
});

/* 
To @execute233: 
 
用户直接注册，然后服务器生成一段文本，复制后丢给管理员，管理员后台审核。 
实现方法可以参照原 CZCA 曾经的网站的代码。 
快写吧😊 
 
*/
/*
 * 6.
 * by execute233.
 * */
