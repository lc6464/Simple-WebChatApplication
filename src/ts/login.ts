import Swal from 'sweetalert2';

import '../css/login.css';

const form: HTMLFormElement = document.querySelector('form'),
	loginButton: HTMLButtonElement = document.querySelector('#login');

form.addEventListener('submit', e => e.preventDefault());

async function fetchData() {
	let message = '';
	try {
		const response = await fetch('/api/user/login', {
			body: new FormData(form),
			method: 'post'
		});
		if (response.ok) {
			try {
				return { success: true, result: await response.json(), message };
			} catch (e) {
				message = '在解析 JSON 过程中发生异常，详细信息请见控制台！';
				console.error('在解析 JSON 过程中发生异常：', e);
			}
		} else {
			message = '在 Fetch 过程中接收到了不成功的状态码，详细信息请见控制台！';
			console.error('在 Fetch 过程中接收到了不成功的状态码，响应对象：', response);
		}
	} catch (e) {
		message = '在 Fetch 过程中发生异常，详细信息请见控制台！';
		console.error('在 Fetch 过程中发生异常：', e);
	}
	return { success: false, result: null, message };
}

loginButton.addEventListener('click', async () => {
	const { success, result, message } = await fetchData();
	if (success) {
		if (result.success) {
			location.href = '/chat';
		} else {
			Swal.fire('登录失败', result.message, 'error');
		}
	} else {
		Swal.fire('登录失败', message, 'warning');
	}
});