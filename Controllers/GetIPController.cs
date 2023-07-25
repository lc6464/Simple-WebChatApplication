using System.Net.Sockets;
using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Controllers;
[ApiController]
[Route("api/[controller]")]
public class GetIPController : ControllerBase {
	private readonly ILogger<GetIPController> _logger;
	private readonly IHttp304 _http304;
	private readonly IHttpConnectionInfo _info;

	public GetIPController(ILogger<GetIPController> logger, IHttpConnectionInfo info, IHttp304 http304) {
		_logger = logger;
		_http304 = http304;
		_info = info;
	}

	[HttpGet]
	[ResponseCache(CacheProfileName = "Private1m")] // 客户端缓存1分钟
	public Models.IP? Get() { // 获取 IP 地址
		var address = _info.RemoteAddress;
		_logger.LogDebug("GetIP: Client {}:{} on {}", address?.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{address}]" : address, _info.RemotePort, _info.Protocol);

		return _http304.TrySet(true, _info.Protocol) ? null : (new(_info));
	}
}