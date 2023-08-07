using SimpleWebChatApplication.Controllers.Models;
using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Controllers;
[ApiController]
[Route("api/[controller]")]
public class RegisterController : ControllerBase {
	private readonly ICheckingTools _tools;
	private readonly IEncryptionTools _encryptionTools;

	public RegisterController(ICheckingTools tools, IEncryptionTools encryptionTools) {
		_tools = tools;
		_encryptionTools = encryptionTools;
	}


	[HttpGet]
	[ResponseCache(CacheProfileName = "NoStore")]
	public RegisterGetResponse Get([FromForm(Name = "access-token")] string? accessToken,
		[FromForm(Name = "register-data")] string? registerData) {

		if (!string.IsNullOrWhiteSpace(accessToken) && HttpContext.Session.GetString("ManageAccessToken") == accessToken) {
			return new() { Code = 6, Success = false, Message = "未登录管理后台。" };
		}
		if (string.IsNullOrWhiteSpace(registerData)) {
			return new() { Code = 5, Success = false, Message = "未提供注册数据。" };
		}
		var registerParts = registerData.Split('/');
		if (registerParts.Length != 2) {
			return new() { Code = 4, Success = false, Message = "无法解析注册数据。" };
		}
		try {
			return _encryptionTools.TryDecryptUserData(registerParts, out var output)
				? new() { Code = 0, Success = true, Data = output }
				: new() { Code = 4, Success = false, Message = "已解析数据，但签名验证失败！" };
		} catch (Exception e) {
			return new() { Code = 4, Success = false, Message = $"无法解析注册数据：{e}" };
		}
	}


	[HttpPost]
	[ResponseCache(CacheProfileName = "NoStore")]
	public RegisterUserPostResponse UserPost([FromForm] string? account, [FromForm] string? password,
		[FromForm(Name = "repeat-password")] string? repeatPassword) {

		if (_tools.IsLogin()) {
			return new() { Success = false, Code = 2, Message = "您当前已登录。" };
		}
		if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(repeatPassword)) {
			return new() { Success = false, Code = 5, Message = "用户名或密码为空。" };
		}
		if (password != repeatPassword) {
			return new() { Success = false, Code = 4, Message = "两次输入的密码不一致。" };
		}
		if (!ICheckingTools.IsPasswordComplicated(password)) {
			return new() {
				Success = false, Code = 8, Message = "密码复杂度不足（最少10位且包含数字、大写字母、小写字母、特殊符号中" +
				"至少三种且每种超过两个）或过长（最多64位）。"
			};
		}
		if (!_tools.IsNameAvailable(account)) {
			return new() { Success = false, Code = 9, Message = "用户名不符合要求或已被占用（4~32位，字母开头，允许数字、字母、下划线、减号）。" };
		}

		RegisterUserPostJsonSerializeTemplate userData = new() {
			Account = account,
			PasswordHash = ICheckingTools.HashPassword(password, out var salt).ToArray(),
			PasswordSalt = salt.ToArray()
		};

		return new() {
			Success = true,
			Code = 0,
			Data = _encryptionTools.EncryptUserData(userData)
		};
	}


	[HttpPost(Name = "import")]
	[ResponseCache(CacheProfileName = "NoStore")]
	public RegisterImportResponse Import([FromForm(Name = "access-token")] string? accessToken,
		[FromForm(Name = "register-data")] string? registerData) {

		if (!string.IsNullOrWhiteSpace(accessToken) && HttpContext.Session.GetString("ManageAccessToken") == accessToken) {
			return new() { Code = 6, Success = false, Message = "未登录管理后台。" };
		}
		if (string.IsNullOrWhiteSpace(registerData)) {
			return new() { Code = 5, Success = false, Message = "未提供注册数据。" };
		}
		var registerParts = registerData.Split('/');
		if (registerParts.Length != 2) {
			return new() { Code = 4, Success = false, Message = "无法解析注册数据。" };
		}
		try {
			if (!_encryptionTools.TryDecryptUserData(registerParts, out var output)) {
				return new() { Code = 4, Success = false, Message = "已解析数据，但签名验证失败！" };
			}
			if (!_tools.IsNameAvailable(output?.Account!)) {
				return new() { Code = 9, Success = false, Message = "用户名不符合要求或已被占用（4~32位，字母开头，允许数字、字母、下划线、减号）。" };
			}

			return new() { Code = 0, Success = true, /*Data = output*/ };
		} catch (Exception e) {
			return new() { Code = 4, Success = false, Message = $"无法解析注册数据：{e}" };
		}
	}
}