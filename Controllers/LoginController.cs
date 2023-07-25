namespace SimpleWebChatApplication.Controllers;
[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase {
	private readonly ILogger<LoginController> _logger;
	private readonly IHttpConnectionInfo _info;

	public LoginController(ILogger<LoginController> logger, IHttpConnectionInfo info) {
		_logger = logger;
		_info = info;
	}

	[HttpPost]
	[ResponseCache(CacheProfileName = "NoStore")]
	public Models.Login Post() {

		return new();
	}
}