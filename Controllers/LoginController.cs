using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Controllers;
[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase {
	private readonly ILogger<LoginController> _logger;
	private readonly IDataProvider _provider;
	private readonly ICheckingTools _tools;

	public LoginController(ILogger<LoginController> logger, IDataProvider provider, ICheckingTools tools) {
		_logger = logger;
		_provider = provider;
		_tools = tools;
	}


	[HttpGet]
	[ResponseCache(CacheProfileName = "NoStore")]
	public Models.Login Get() => _tools.IsLogin(out var displayName)
			? new() { Success = true, Code = 0, Message = "已登录。", DisplayName = displayName }
			: new() { Success = false, Code = 3, Message = "未登录。" };


	[HttpPost]
	[ResponseCache(CacheProfileName = "NoStore")]
	public Models.Login Post([FromForm] string? account, [FromForm] string? password) {
		if (_tools.IsLogin()) {
			return new() { Success = true, Code = 1, Message = "您已经登录过了。" };
		}
		if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password)) {
			return new() { Success = false, Code = 5, Message = "用户名或密码为空。" };
		}
		_ = Hubs.Cache.MemoryCache.TryGetValue($"TryLoginCount of {account}", out int count);
		if (count > 5) {
			_logger.LogWarning("用户 {} 尝试登录次数过多，最后一次 IP 地址为 {}。", account, HttpContext.Connection.RemoteIpAddress);
			return new() { Success = false, Code = 7, Message = "尝试登录次数过多，请在30分钟后重试。" };
		}
		if (account.Length is < 4 or > 32 || !ICheckingTools.IsPasswordComplicated(password)) {
			_ = Hubs.Cache.Set($"TryLoginCount of {account}", ++count, TimeSpan.FromMinutes(30), TimeSpan.FromHours(2));
			return new() { Success = false, Code = 6, Message = "用户名或密码错误。" };
		}
		using var reader = _provider.GetUserReader(account);
		if (!reader.Read()) {
			_ = Hubs.Cache.Set($"TryLoginCount of {account}", ++count, TimeSpan.FromMinutes(30), TimeSpan.FromHours(2));
			return new() { Success = false, Code = 6, Message = "用户名或密码错误。" };
		}
		var hash = new byte[64];
		_ = reader.GetBytes(3, 0, hash, 0, 64);
		var salt = new byte[16];
		_ = reader.GetBytes(4, 0, salt, 0, 16);
		if (!ICheckingTools.VerifyPassword(password, hash, salt)) {
			_ = Hubs.Cache.Set($"TryLoginCount of {account}", ++count, TimeSpan.FromMinutes(30), TimeSpan.FromHours(2));
			return new() { Success = false, Code = 6, Message = "用户名或密码错误。" };
		}
		Hubs.Cache.MemoryCache.Remove($"TryLoginCount of {account}");
		HttpContext.Session.SetString("Name", account);
		HttpContext.Session.SetString("Nick", reader.GetString(2));
		HttpContext.Session.Set("Hash", hash);
		HttpContext.Session.Set("Salt", salt);
		return new() { Success = true, Code = 0, Message = "登录成功！" };
	}
}