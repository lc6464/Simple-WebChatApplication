﻿using SimpleWebChatApplication.Controllers.Models;
using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Controllers;
[ApiController]
[Route("api/[controller]")]
public class RegisterController : ControllerBase {
	private readonly IGeneralTools _tools;
	private readonly IEncryptionTools _encryptionTools;
	private readonly IDataProvider _provider;
	private readonly ILogger<RegisterController> _logger;
	private readonly IHttpConnectionInfo _info;

	public RegisterController(IGeneralTools tools, IEncryptionTools encryptionTools, IDataProvider provider, ILogger<RegisterController> logger, IHttpConnectionInfo info) {
		_tools = tools;
		_encryptionTools = encryptionTools;
		_provider = provider;
		_logger = logger;
		_info = info;
	}


	[HttpGet]
	[ResponseCache(CacheProfileName = "NoStore")]
	public RegisterGetResponse Get([FromForm(Name = "access-token")] string? accessToken,
		[FromForm(Name = "register-data")] string? registerData) {

		_logger.LogDebug("Get: access-token: {}, register-data: {}, IP: {}", accessToken, registerData, _info.RemoteAddress);
		if (string.IsNullOrWhiteSpace(accessToken) || HttpContext.Session.GetString("ManageAccessToken") != accessToken) {
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
			if (_encryptionTools.TryDecryptUserData(registerParts, out var output)) {
				return new() { Code = 0, Success = true, Data = output };
			}
			_logger.LogWarning("Get: 签名验证失败：{}", registerData);
			return new() { Code = 4, Success = false, Message = "已解析数据，但签名验证失败！" };
		} catch (Exception e) {
			_logger.LogInformation("Get: 无法解析注册数据：{}", e);
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
		if (!IGeneralTools.IsPasswordComplicated(password)) {
			return new() {
				Success = false, Code = 8,
				Message = "密码复杂度不足（最少10位且包含数字、大写字母、小写字母、特殊符号中" +
					"至少三种且每种超过两个）或过长（最多64位）。"
			};
		}
		if (!_tools.IsNameAvailable(account)) {
			return new() { Success = false, Code = 9, Message = "用户名不符合要求或已被占用（4~32位，字母开头，允许数字、字母、下划线、减号）。" };
		}

		RegisterUserPostJsonSerializeTemplate userData = new() {
			Account = account,
			PasswordHash = IGeneralTools.HashPassword(password, out var salt).ToArray(),
			PasswordSalt = salt.ToArray()
		};
		var encryptUserData = _encryptionTools.EncryptUserData(userData);
		_logger.LogDebug("UserPost: Encrypt user {} data success! IP: {}", account, _info.RemoteAddress);
		return new() {
			Success = true,
			Code = 0,
			Data = encryptUserData
		};
	}


	[HttpPost("import")]
	[ResponseCache(CacheProfileName = "NoStore")]
	public RegisterImportResponse Import([FromForm(Name = "access-token")] string? accessToken,
		[FromForm(Name = "register-data")] string? registerData) {

		_logger.LogDebug("Import: access-token: {}, register-data: {}, IP: {}", accessToken, registerData, _info.RemoteAddress);
		if (string.IsNullOrWhiteSpace(accessToken) || HttpContext.Session.GetString("ManageAccessToken") != accessToken) {
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
			if (!_encryptionTools.TryDecryptUserData(registerParts, out var nullableOutput)) {
				_logger.LogWarning("Import: 签名验证失败：{}", registerData);
				return new() { Code = 4, Success = false, Message = "已解析数据，但签名验证失败！" };
			}
			var output = (RegisterGetResponseUserData)nullableOutput!;
			if (!_tools.IsNameAvailable(output.Account)) {
				return new() { Code = 9, Success = false, Message = $"用户名 {output.Account} 不符合要求或已被占用（4~32位，字母开头，允许数字、字母、下划线、减号）。" };
			}
			try {
				using var transaction = _provider.Connection.BeginTransaction();
				using var command = _provider.Connection.CreateCommand();
				command.Transaction = transaction;
				command.CommandText = "INSERT INTO Users (Name, Nick, Hash, Salt, RegisterTime, ImportTime) VALUES (@account, @nick, @hash, @salt, @rT, @iT);";
				command.Parameters.AddWithValue("@account", output.Account);
				command.Parameters.AddWithValue("@nick", "");
				command.Parameters.AddWithValue("@hash", output.PasswordHash);
				command.Parameters.AddWithValue("@salt", output.PasswordSalt);
				command.Parameters.AddWithValue("@rT", output.Timestamp);
				command.Parameters.AddWithValue("@iT", IGeneralTools.Timestamp);
				command.ExecuteNonQuery();
				using var cmdQuery = _provider.Connection.CreateCommand();
				cmdQuery.Transaction = transaction;
				cmdQuery.CommandText = "SELECT ID, ImportTime FROM Users WHERE Name = @account;";
				cmdQuery.Parameters.AddWithValue("@account", output.Account);
				using var reader = cmdQuery.ExecuteReader();
				if (!reader.Read()) {
					_logger.LogError("Import: 数据库异常！用户 {} 似乎已导入却无法查询到相关数据！", output.Account);
					return new() { Code = 10, Success = false, Message = "数据库异常！似乎已导入却无法查询到相关数据！" };
				}
				var id = reader.GetInt64(0);
				var importTime = reader.GetInt64(1);
				transaction.Commit();
				return new() { Code = 0, Success = true, Data = new(output, id, importTime) };
			} catch (Exception e) {
				_logger.LogError("Import: 写入注册数据到数据库失败：{}", e);
				return new() { Code = 10, Success = false, Message = $"写入注册数据到数据库失败：{e}" };
			}
		} catch (Exception e) {
			_logger.LogInformation("Import: 无法解析注册数据：{}", e);
			return new() { Code = 4, Success = false, Message = $"无法解析注册数据：{e}" };
		}
	}
}