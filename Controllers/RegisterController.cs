using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Controllers;
[ApiController]
[Route("api/[controller]")]
public class RegisterController : ControllerBase {
	private readonly ILogger<RegisterController> _logger;
	private readonly ICheckingTools _tools;
	private readonly IConfiguration _configuration;

	public RegisterController(ILogger<RegisterController> logger, ICheckingTools tools, IConfiguration configuration) {
		_logger = logger;
		_tools = tools;
		_configuration = configuration;
	}


	[HttpGet]
	[ResponseCache(CacheProfileName = "NoStore")]
	public Models.Login Get() => _tools.IsLogin(out var displayName)
			? new() { Success = true, Code = 0, Message = "已登录。", DisplayName = displayName }
			: new() { Success = false, Code = 3, Message = "未登录。" };


	[HttpPost]
	[ResponseCache(CacheProfileName = "NoStore")]
	public Models.Login Post(string? account, string? password, string? repeatPassword) {
		if (_tools.IsLogin()) {
			return new() { Success = false, Code = 2, Message = "您当前已登录。" };
		}
		if (account is null || password is null || repeatPassword is null) {
			return new() { Success = false, Code = 5, Message = "用户名或密码为空。" };
		}
		if (password != repeatPassword) {
			return new() { Success = false, Code = 4, Message = "两次输入的密码不一致。" };
		}
		if (!ICheckingTools.IsPasswordComplicated(password)) {
			return new() { Success = false, Code = 8, Message = "密码复杂度不足（最少10位且须足够复杂）或过长（最多64位）。" };
		}
		if (_tools.IsNameAvailable(account)) {
			return new() { Success = false, Code = 9, Message = "用户名不符合要求或已被占用（4~32位）。" };
		}
		return new() { Success = true, Code = 0, Message = "登录成功！" };
	}
}