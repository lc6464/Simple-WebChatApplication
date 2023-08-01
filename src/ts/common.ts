export async function fetchText(
	input: URL | string,
	initOrAsJson?: RequestInit,
): Promise<{
	success: boolean;
	result: {
		success: boolean;
		code: number;
		message?: string;
		data?: unknown;
		displayName?: string;
	};
	message?: string;
}>;
export async function fetchText(
	input: URL | string,
	initOrAsJson?: boolean,
): Promise<{
	success: boolean;
	result: {
		success: boolean;
		code: number;
		message?: string;
		data?: unknown;
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
	result: {
		success: boolean;
		code: number;
		message?: string;
		data?: unknown;
		displayName?: string;
	};
	message: string | null;
}>;
export async function fetchText(
	input: RequestInfo,
	initOrAsJson?: boolean,
): Promise<{
	success: boolean;
	result: {
		success: boolean;
		code: number;
		message?: string;
		data?: unknown;
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
	result: {
		success: boolean;
		code: number;
		message?: string;
		data?: unknown;
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
