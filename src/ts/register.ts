import Swal from 'sweetalert2/dist/sweetalert2.min.js';

import '../css/login.css';

document.querySelector('html').addEventListener('click', e => {
	const target = e.target as HTMLElement;
	if (!(target instanceof HTMLAnchorElement && target.href?.endsWith("execute233"))) {
		e.preventDefault();
		Swal.fire({
			title: '这里啥也没有',
			text: '请联系 GitHub @execute233 早日完成！',
			icon: 'question',
			footer: '<a href="https://github.com/execute233" target="_blank">GitHub @execute233</a>'
		});
	}
});


/*
To @execute233:

用户直接注册，然后服务器生成一段文本，复制后丢给管理员，管理员后台审核。
实现方法可以参照原 CZCA 曾经的网站的代码。
快写吧😊

*/