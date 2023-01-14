using Microsoft.AspNetCore.Diagnostics;

namespace SimpleWebChatApplication.Pages;
[ResponseCache(NoStore = true)]
[IgnoreAntiforgeryToken]
public class StatusCodeModel : PageModel {
	public int OriginalStatusCode { get; set; }
	public string? OriginalPathAndQuery { get; set; }

	public string? Title { get; set; }
	public string? Message { get; set; }

	public void OnGet(int statusCode) {
		OriginalStatusCode = statusCode;

		var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

		if (statusCodeReExecuteFeature is null) {
			Response.StatusCode = statusCode = 404;
			OriginalPathAndQuery = Request.PathBase + Request.Path + Request.QueryString;
		} else {
			OriginalPathAndQuery = statusCodeReExecuteFeature.OriginalPathBase
				+ statusCodeReExecuteFeature.OriginalPath + statusCodeReExecuteFeature.OriginalQueryString;
		}

		ViewData["Title"] = Title = statusCode switch {
			400 => "400 - Bad Request",
			401 => "401 - Unauthorized",
			402 => "402 - Payment Required",
			403 => "403 - Forbidden",
			404 => "404 - Not Found",
			405 => "405 - Method Not Allowed",
			406 => "406 - Not Acceptable",
			407 => "407 - Proxy Authentication Required",
			408 => "408 - Request Timeout",
			409 => "409 - Conflict",
			410 => "410 - Gone",
			411 => "411 - Length Required",
			412 => "412 - Precondition Failed",
			413 => "413 - Payload Too Large",
			414 => "414 - Uri Too Long",
			415 => "415 - Unsupported Media Type",
			416 => "416 - Range Not Satisfiable",
			417 => "417 - Expectation Failed",
			418 => "418 - I'm a teapot",
			421 => "421 - Misdirected Request",
			422 => "422 - Unprocessable Entity",
			423 => "423 - Locked",
			424 => "424 - Failed Dependency",
			425 => "425 - Too Early",
			426 => "426 - Upgrade Required",
			428 => "428 - Precondition Required",
			429 => "429 - Too Many Requests",
			431 => "431 - Request Header Fields Too Large",
			451 => "451 - Unavailable For Legal Reasons",

			500 => "500 - Internal Server Error",
			501 => "501 - Not Implemented",
			502 => "502 - Bad Gateway",
			503 => "503 - Service Unavailable",
			504 => "504 - Gateway Timeout",
			505 => "505 - HTTP Version Not Supported",
			506 => "506 - Variant Also Negotiates",
			507 => "507 - Insufficient Storage",
			508 => "508 - Loop Detected",
			510 => "510 - Not Extended",
			511 => "511 - Network Authentication Required",

			_ => $"{statusCode} | Status Code Not Found",
		};

		ViewData["Keywords"] = "LC,网站,测试,错误,HTTP 状态码";

		ViewData["Description"] = Message = statusCode switch {
			400 => "请求语法错误，服务器无法理解！",
			401 => "未经授权，请完成身份认证！",
			402 => "此状态码被保留，将来可能会被用于支付！",
			403 => "您没有权限访问此页面！",
			404 => "您请求的内容不存在！",
			405 => "您使用的请求方法不被允许！",
			406 => "没有发现任何符合浏览器给定标准的内容！",
			407 => "未经授权，请使用代理完成身份认证！",
			408 => "请求超时！",
			409 => "请求与服务器的当前状态冲突！",
			410 => "您请求的内容已被永久移除！",
			411 => "请提供 Content-Length 请求头！",
			412 => "服务器未能满足客户端指出的先决条件！",
			413 => "请求的负载数据过大！",
			414 => "请求的 URI 过长！",
			415 => "服务器不支持请求数据的媒体格式！",
			416 => "无法满足请求中 Range 标头字段指定的范围，该范围可能超出了目标 URI 数据的大小！",
			417 => "服务器无法满足 Expect 请求标头字段所指示的期望！",
			418 => "请不要用茶壶冲泡咖啡哟~",
			421 => "请求被定向到无法生成响应的服务器！",
			422 => "请求格式正确，但由于语义错误而无法遵循。",
			423 => "正在访问的资源已锁定。",
			424 => "由于依赖的请求失败而导致本次请求失败！",
			425 => "服务器不愿意冒险处理可能被重播的请求！",
			426 => "服务器拒绝使用当前协议执行请求，但在客户端升级到其他协议后可能愿意这样做！出现此状态码可能是您的浏览器不支持 WebSocket 导致的！",
			428 => "源服务器要求请求是有条件的。",
			429 => "用户在给定的时间内发送了太多请求！",
			431 => "请求头过大！",
			451 => "您请求了无法合法提供的资源！",

			500 => "服务器遇到了不知道如何处理的情况，请联系站长处理！",
			501 => "服务器不支持您使用的请求方法，因此无法处理。",
			502 => "服务器作为网关需要得到上游服务器的一个处理这个请求的响应，但是得到一个错误的响应。",
			503 => "服务器没有准备好处理请求，服务器可能因维护或重载而停机。",
			504 => "服务器作为网关未能在超时时间内得到上游服务器的响应！",
			505 => "您所使用的 HTTP 版本不受服务器支持！",
			506 => "服务器存在内部配置错误，请联系站长处理！",
			507 => "无法在资源上执行该方法，因为服务器无法存储成功完成请求所需的表示！",
			508 => "服务器在处理请求时检测到无限循环！",
			510 => "服务器需要对请求进行进一步扩展才能完成请求！",
			511 => "客户端需要进行身份验证才能获得网络访问权限！",

			_ => "我们无法确认当前的状态码的意义，未能判断发生了什么！",
		};
	}
}