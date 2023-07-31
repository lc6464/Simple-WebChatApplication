export async function fetchText(input: URL | string, initOrAsJson?: RequestInit)
export async function fetchText(input: URL | string, initOrAsJson?: boolean)
export async function fetchText(input: URL | string, initOrAsJson?: RequestInit, asJson?: boolean)
export async function fetchText(input: RequestInfo, initOrAsJson?: boolean)
export async function fetchText(input: URL | string | RequestInfo, initOrAsJson?: RequestInit | boolean, asJson?: boolean) {
	let message = '';
	try {
		let response: Response;
		if (typeof initOrAsJson === 'boolean') {
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
				return { success: true, result: await (asJson ? response.json() : response.text()), message };
			} catch (e) {
				message = `在解析${asJson ? " JSON " : "文本"}过程中发生异常，详细信息请见控制台！`;
				console.error('在解析 JSON 过程中发生异常：', e);
			}
		} else {
			message = '在 Fetch 过程中接收到了不成功的状态码，详细信息请见控制台！';
			console.error('在 Fetch 过程中接收到了不成功的状态码，响应对象：', response);
			return { success: false, result: response, message };
		}
	} catch (e) {
		message = '在 Fetch 过程中发生异常，详细信息请见控制台！';
		console.error('在 Fetch 过程中发生异常：', e);
	}
	return { success: false, result: null, message };
}



export async function copyText(text: string) {
	let result = false;
	try {
		await navigator.clipboard.writeText(text); // 尝试使用 Clipboard API
		result = true;
	} catch (err) {
		console.error('尝试使用 navigator.clipboard.writeText 复制失败：', err);
		const input = document.createElement('input');
		input.readOnly = true;
		input.style.position = 'fixed';
		input.style.top = '-9999em';
		input.style.height = '0';
		input.value = text;
		document.body.appendChild(input);
		input.select();
		input.setSelectionRange(0, 99999);
		// @ts-ignore
		if (document.execCommand !== null && document.execCommand('copy')) {
			result = true;
			console.log('使用 document.execCommand 复制成功。');
		} else {
			console.error('使用 document.execCommand 复制失败，document.execCommand 是否存在：', document.execCommand !== null ? '是' : '否');
		}
		document.body.removeChild(input);
	}
	return result;
}