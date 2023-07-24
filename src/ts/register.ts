import Swal from 'sweetalert2/dist/sweetalert2.min.js';

import '../css/login.css';

document.querySelector('html').addEventListener('click', e => {
	e.preventDefault();
	Swal.fire({
		title: '这里啥也没有',
		html: '请联系 GitHub @execute233 早日完成！',
		icon: 'question',
		footer: '<a href="https://github.com/execute233">GitHub @execute233</a>'
	});
});


/*
To @execute233:

我打算采用邀请注册制。
你觉得后台生成邀请码，丢给用户注册好；
还是后台生成 ID 和临时密码，丢给用户注册好；
还是用户直接注册，然后服务器生成一段文本，复制后丢给管理员，管理员后台审核；
抑或是不允许注册，由管理员直接在后台添加用户？

*/