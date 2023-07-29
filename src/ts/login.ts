import Swal from 'sweetalert2/dist/sweetalert2.min.js';

import '../css/login.css';

const form: HTMLFormElement = document.querySelector('form'),
	loginButton: HTMLButtonElement = document.querySelector('#login'),
	resetPasswordAnchor: HTMLAnchorElement = document.querySelector('#reset-password');

form.addEventListener('submit', e => e.preventDefault());
resetPasswordAnchor.addEventListener('click', e => {
	e.preventDefault();
	Swal.fire({
		title: '重置密码',
		text: '请联系管理员重置密码。',
		icon: 'info',
		footer: '<a href="contact" title="联系站长">点此联系站长</a>'
	});
});

async function fetchData() {
	let message = '';
	try {
		const response = await fetch('api/login', {
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
			Swal.fire('登录成功', '即将跳转到聊天页面。', 'success').then(() => location.href = 'chat.html');
		} else {
			Swal.fire({
				title: '登录失败',
				text: result.message,
				icon: 'error',
				footer: '<a href="contact" title="联系站长">点此联系站长</a>'
			});
		}
	} else {
		Swal.fire('登录失败', message, 'warning');
	}
});