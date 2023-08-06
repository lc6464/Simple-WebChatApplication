export async function fetchText(
	input: URL | string,
	initOrAsJson?: RequestInit,
): Promise<{
	success: boolean;
	result:
		| Response
		| {
				success: boolean;
				code: number;
				message?: string;
				data?: string;
				displayName?: string;
		  };
	message?: string;
}>;
export async function fetchText(
	input: URL | string | RequestInfo,
	initOrAsJson?: boolean,
): Promise<{
	success: boolean;
	result:
		| Response
		| {
				success: boolean;
				code: number;
				message?: string;
				data?: string;
				displayName?: string;
		  };
	message: string | null;
}>;
export async function fetchText(
	input: URL | string,
	initOrAsJson?: RequestInit,
	asJson?: boolean,
): Promise<{
	success: boolean;
	result:
		| Response
		| {
				success: boolean;
				code: number;
				message?: string;
				data?: string;
				displayName?: string;
		  };
	message: string | null;
}>;
export async function fetchText(
	input: URL | string | RequestInfo,
	initOrAsJson?: RequestInit | boolean,
	asJson?: boolean,
): Promise<{
	success: boolean;
	result:
		| Response
		| {
				success: boolean;
				code: number;
				message?: string;
				data?: string;
				displayName?: string;
		  };
	message: string | null;
}> {
	let message = "";
	try {
		let response: Response;
		if (typeof initOrAsJson === "boolean") {
			asJson = initOrAsJson ?? asJson ?? true;
			response = await fetch(input);
		} else if (initOrAsJson !== null) {
			response = await fetch(input, initOrAsJson);
		} else {
			response = await fetch(input);
		}
		asJson = asJson ?? true;
		if (response.ok) {
			try {
				return {
					success: true,
					result: await (asJson ? response.json() : response.text()),
					message,
				};
			} catch (e) {
				message = `在解析${
					asJson ? " JSON " : "文本"
				}过程中发生异常，详细信息请见控制台！`;
				console.error("在解析 JSON 过程中发生异常：", e);
			}
		} else {
			message =
				"在 Fetch 过程中接收到了不成功的状态码，详细信息请见控制台！";
			console.error(
				"在 Fetch 过程中接收到了不成功的状态码，响应对象：",
				response,
			);
			return { success: false, result: response, message };
		}
	} catch (e) {
		message = "在 Fetch 过程中发生异常，详细信息请见控制台！";
		console.error("在 Fetch 过程中发生异常：", e);
	}
	return { success: false, result: null, message };
}

export async function copyText(text: string) {
	let result = false;
	try {
		await navigator.clipboard.writeText(text); // 尝试使用 Clipboard API，若不存在或复制失败则抛出异常
		result = true;
	} catch (err) {
		// Non-Local HTTP Fallback
		console.error("尝试使用 navigator.clipboard.writeText 复制失败：", err);
		const input = document.createElement("input");
		input.readOnly = true;
		input.style.position = "fixed";
		input.style.top = "-9999em";
		input.style.height = "0";
		input.value = text;
		document.body.appendChild(input);
		input.select();
		input.setSelectionRange(0, 99999);
		if (document.execCommand !== null && document.execCommand("copy")) {
			result = true;
			console.log("使用 document.execCommand 复制成功。");
		} else {
			console.error(
				"使用 document.execCommand 复制失败，document.execCommand 是否存在：",
				document.execCommand !== null ? "是" : "否",
			);
		}
		document.body.removeChild(input);
	}
	return result;
}

export function randomUUID() {
	if (typeof crypto.randomUUID === "function") {
		return crypto.randomUUID();
	}
	return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, (c) => {
		const num = Number(c);
		return (
			num ^
			(crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> (num / 4)))
		).toString(16);
	});
}
export function formatTime(time: Date) {
	const pL = (n: number) => n.toString().padStart(2, "0");
	return `${time.getFullYear()}-${
		time.getMonth() + 1
	}-${time.getDate()} ${time.getHours()}:${pL(time.getMinutes())}:${pL(
		time.getSeconds(),
	)}`;
}
//验证密码是否足够复杂
export function isPasswordComplicated(password: string) {
	if (password.length < 10 || password.length > 64) {
		return {result: false, message: "密码长度必须大于或等于10个字符且小于或等于64个字符"};
	}
	if (repeatRegex.test(password)) {
		return {result: false, message: "不允许重复出现4个及以上的字符"};
	}
	let kind: number = 0;
	//特殊字符
	if (kindConfirm(symbolRegex, password)) {
		kind++;
	}
	//小写
	if (kindConfirm(lowerLetterRegex, password)) {
		kind++;
	}
	//大写
	if (kindConfirm(upperLetterRegex, password)) {
		kind++;
	}
	//数字
	if (kindConfirm(numberRegex, password)) {
		kind++;
	}
	
	return {
		result: kind > 2, 
		message: kind > 2 ? 
			"true" : 
			"密码必须包含大写字母，小写字母，特殊字符和数字任意3种及以上，且每种包含的字符必须超过2个及以上"
	};
}
//验证密码是否符合匹配两次及以上
export function kindConfirm(regex: RegExp, password: string) {
	return password.match(regex).length > 1;
}
const repeatRegex: RegExp = /(?<a>.)\k<a>{3}/g;
const symbolRegex: RegExp = /[`~!@#$%^&*()_+=\[{\]};:'"<>|./\\?,\-]/g;
const lowerLetterRegex: RegExp = /[a-z]/g;
const upperLetterRegex: RegExp = /[A-Z]/g;
const numberRegex: RegExp = /\d/g;

//验证用户名是否符合规范
export function isNameAvailable(name: string) {
	return name.length >= 4 && name.length <= 32 && nameRegex.test(name);
}
const nameRegex: RegExp = /^[A-Za-z][A-Za-z\d\-_]+$/g;