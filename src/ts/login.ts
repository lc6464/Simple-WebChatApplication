import Swal from 'sweetalert2/dist/sweetalert2.min.js';

import '../css/login.css';
import {fetchData} from "./common";

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

loginButton.addEventListener('click', async () => {
	const { success, result, message } = await fetchData('api/login', {
		body: new FormData(form),
		method: 'post'
	});
});