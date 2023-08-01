//import { fetchText } from "./common";
/*
 * 请注意，这两个方法都可能需要访问外部链接，理论上来说只有正确设置Content-Security-Policy才能用。
 * 当然，这两个方法我也没测试过，也不知道能不能用。
 * */
/*
export async function getBingImageUrl() {
	let result: string = null;
	try {
		const { success, result, message } = await fetchText(
			"https://cn.bing.com/HPImageArchive.aspx?format=js&idx=0&n=1&mkt=zh-CN",
		);
		if (result.ok) {
			const url = "https://cn.bing.com" + result.images.url;
		} else {
			console.warn("您貌似没有联网,所以你不能有来自必应每日壁纸的背景");
		}
	} catch (e) {
		console.warn("尝试获取背景图片时发生了未知的问题", e);
	}
	return result;
}
export async function loadBackgroundImageFromBing() {
	const url = await getBingImageUrl();
	if (url !== null) {
		document.body.style.background =
			"url(" +
			url +
			");background-size: 100% ;background-attachment: fixed";
	}
}
*/