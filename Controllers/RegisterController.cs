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


	/*
	[HttpGet]
	[ResponseCache(CacheProfileName = "NoStore")]
	public Models.RegisterGet Get() { }
	*/

	[HttpPost]
	[ResponseCache(CacheProfileName = "NoStore")]
	public Register Post([FromForm] string? account, [FromForm] string? password, [FromForm(Name = "repeat-password")] string? repeatPassword) {
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
			return new() { Success = false, Code = 8, Message = "密码复杂度不足（最少10位且须足够复杂）或过长（最多64位）。" };
		}
		if (!_tools.IsNameAvailable(account)) {
			return new() { Success = false, Code = 9, Message = "用户名不符合要求或已被占用（4~32位，字母开头，允许数字、字母、下划线、减号）。" };
		}
		RegisterUserData userData = new() {
			Account = account,
			PasswordHash = ICheckingTools.HashPassword(password, out var salt).ToArray(),
			PasswordSalt = salt.ToArray()
		};

		return new() {
			Success = true,
			Code = 0,
			Message = "已成功注册！请将展示的数据复制后发送给网站管理员，待管理员审核通过后即可登录。",
			Data = _encryptionTools.EncryptUserData(userData)
		};
	}
}