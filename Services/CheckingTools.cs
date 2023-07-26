﻿using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取数据库的类。
/// </summary>
public class CheckingTools : ICheckingTools {
	private readonly HttpContext HttpContext;
	private ISession Session => HttpContext.Session;
	private readonly IDataProvider _provider;
	private SqliteDataReader GetUserReader(string name) => _provider.GetUserReader(name);


	/// <summary>
	/// 默认构造函数。
	/// </summary>
	/// <param name="accessor">HttpContextAccessor</param>
	public CheckingTools(IHttpContextAccessor accessor, IDataProvider provider) {
		HttpContext = accessor.HttpContext!;
		_provider = provider;
	}

	

	/// <summary>
	/// 检查是否登录。
	/// </summary>
	/// <returns>若已登录，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public bool IsLogin() {
		var account = Session.GetString("Name");
		if (account is null) {
			Session.Clear();
			return false;
		}
		using var reader = GetUserReader(account);
		if (!reader.HasRows) {
			Session.Clear();
			return false;
		}
		var hash = new byte[64];
		reader.GetBytes(3, 0, hash, 0, 64);
		var salt = new byte[16];
		reader.GetBytes(4, 0, salt, 0, 16);
		var sessionHash = Session.Get("Hash");
		var sessionSalt = Session.Get("Salt");
		if (sessionHash is null || sessionSalt is null) {
			Session.Clear();
			return false;
		}
		if (!hash.SequenceEqual(sessionHash) || !salt.SequenceEqual(sessionSalt)) {
			Session.Clear();
			return false;
		}
		Session.SetString("Nick", reader.GetString(2));
		return true;
	}
}