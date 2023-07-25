namespace SimpleWebChatApplication.Controllers;
[ApiController]
[Route("api/[controller]")]
public class InstallController : ControllerBase {
	private readonly ILogger<InstallController> _logger;

	public InstallController(ILogger<InstallController> logger) => _logger = logger;

	[HttpPost]
	[ResponseCache(CacheProfileName = "NoStore")]
	public void Post() {

	}
}