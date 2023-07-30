import Swal from 'sweetalert2/dist/sweetalert2.min.js';

/*
* @lc6464，
* 快来整合你写的代码。
* */
/*
* 快捷请求数据？
* @return 
*   success: 请求是否成功
*   result: 响应体（可能是string对象（默认）也可能是json对象）
*   message: 
* */
export async function fetchData(endPoint, options, asJson = true) {
    let message = '';
    try {
        const response = await fetch(endPoint, options);
        if (response.ok) {
            try {
                return { success: true, result: await (asJson ? response.json() : response.text()), message };
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

//复制内容到系统剪切板.
//@return boolean 复制是否成功.
export async function copy(text: string) {
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
        input.setSelectionRange(0, 9999);
        if (document.execCommand != null && document.execCommand('copy')) {
            result = true;
            console.log('使用 document.execCommand 复制成功。');
        } else {
            console.error('使用 document.execCommand 复制失败，document.execCommand 是否存在：', document.execCommand != null ? '是' : '否');
        }
        document.body.removeChild(input);
    }
    return result;
}