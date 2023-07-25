using System.Net.Sockets;
using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Controllers;
[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase {
	private readonly ILogger<HelloController> _logger;
	private readonly IBackendConfiguration _info;

	public HelloController(ILogger<HelloController> logger, IBackendConfiguration info) {
		_logger = logger;
		_info = info;
	}

	[HttpGet]
	[ResponseCache(CacheProfileName = "NoStore")]
	public Models.Hello Get() { // 打个招呼
		var address = _info.RemoteAddress;
		_logger.LogDebug("Hello! Client {}:{} on {}", address?.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{address}]" : address, _info.RemotePort, _info.Protocol);
		return new(_info);
	}
}