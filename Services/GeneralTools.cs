using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 常用工具类。
/// </summary>
public partial class GeneralTools : IGeneralTools {
	private readonly HttpContext _httpContext;
	private ISession Session => _httpContext.Session;
	private readonly IDataProvider _provider;
	private readonly ILogger<GeneralTools> _logger;
	private readonly IHttpConnectionInfo _info;
	private SqliteDataReader GetUserReader(string? name, out SqliteCommand cmd) => _provider.GetUserReader(name, out cmd);


	/// <summary>
	/// 默认构造函数。
	/// </summary>
	/// <param name="accessor">注入的 <see cref="IHttpContextAccessor"/></param>
	/// <param name="provider">注入的 <see cref="IDataProvider"/></param>
	/// <param name="logger">注入的 <see cref="ILogger"/></param>
	/// <param name="info">注入的 <see cref="IHttpConnectionInfo"/></param>
	public GeneralTools(IHttpContextAccessor accessor, IDataProvider provider, ILogger<GeneralTools> logger, IHttpConnectionInfo info) {
		_httpContext = accessor.HttpContext!;
		_provider = provider;
		_logger = logger;
		_info = info;
	}


	/// <summary>
	/// 检查用户名是否可用。
	/// </summary>
	/// <param name="name">用户名</param>
	/// <returns>若可用则为 <see langword="false"/>，否则为 <see langword="true"/>。</returns>
	public bool IsNameAvailable(string? name) {
		if (string.IsNullOrWhiteSpace(name) || name.Length is < 4 or > 32 || !NameRegex().IsMatch(name)) {
			return false;
		}

		var result = !GetUserReader(name, out var cmd).HasRows;
		cmd.Dispose();
		return result;
	}


	/// <summary>
	/// 检查是否登录。
	/// </summary>
	/// <returns>若已登录，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public bool IsLogin() => IsLogin(out _);


	/// <summary>
	/// 检查是否登录。
	/// </summary>
	/// <param name="displayName">输出显示的用户名。</param>
	/// <returns>若已登录，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public bool IsLogin(out string displayName) {
		displayName = "";
		var account = Session.GetString("Name");
		if (account is null) {
			Session.Clear();
			return false;
		}
		using var reader = GetUserReader(account, out var cmd);
		if (!reader.Read()) {
			_logger.LogWarning("IsLogin: {} 于 {} 在数据库中不存在，可能是此用户在其他地方注销了账号！", account, _info.RemoteAddress);
			Session.Clear();
			return false;
		}
		var hash = new byte[64];
		_ = reader.GetBytes(3, 0, hash, 0, 64);
		var salt = new byte[16];
		_ = reader.GetBytes(4, 0, salt, 0, 16);
		var sessionHash = Session.Get("Hash");
		var sessionSalt = Session.Get("Salt");
		if (sessionHash is null || sessionSalt is null) {
			_logger.LogWarning("IsLogin: {} 于 {} 在 Session 中没有 Hash 或 Salt，此现象不符合应用程序正常运行预期！", account, _info.RemoteAddress);
			Session.Clear();
			return false;
		}
		if (!hash.SequenceEqual(sessionHash) || !salt.SequenceEqual(sessionSalt)) {
			_logger.LogWarning("IsLogin: {} 于 {} 的 Session Hash 或 Salt 与数据库中的不匹配，可能是此用户在其他地方修改了密码！", account, _info.RemoteAddress);
			Session.Clear();
			return false;
		}
		var nick = reader.GetString(2);
		Session.SetString("Nick", nick);
		displayName = $"{nick} ({account})";
		cmd.Dispose();
		return true;
	}



	[GeneratedRegex(@"^[A-Za-z][A-Za-z\d\-_]+$")]
	private static partial Regex NameRegex();
}