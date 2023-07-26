using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取数据库的类。
/// </summary>
public partial class CheckingTools : ICheckingTools {
	private readonly IHttpContextAccessor _accessor;
	private HttpContext HttpContext => _accessor.HttpContext!;
	private ISession Session => HttpContext.Session;
	private readonly IDataProvider _provider;
	private SqliteConnection Connection => _provider.Connection;

	/// <summary>
	/// 默认构造函数。
	/// </summary>
	/// <param name="accessor">HttpContextAccessor</param>
	public CheckingTools(IHttpContextAccessor accessor, IDataProvider provider) {
		_accessor = accessor;
		_provider = provider;
	}

	/// <summary>
	/// 检查用户名是否可用。
	/// </summary>
	/// <param name="name">用户名</param>
	/// <returns>若被使用或长度不合适，则为 <see langword="false"/>，若可用则为 <see langword="true"/>。</returns>
	public bool IsNameAvailable(string name) {
		if (name.Length < 4 || name.Length > 32) {
			return false;
		}
		using var transaction = Connection.BeginTransaction();
		using var cmd = Connection.CreateCommand();
		cmd.Transaction = transaction;
		cmd.CommandText = $"SELECT * FROM Users WHERE Name = '@Name'";
		cmd.Parameters.AddWithValue("@Name", name);
		var reader = _provider.CmdExeReader(cmd);
		transaction.Commit();
		return !reader.HasRows;
	}

	/// <summary>
	/// 检查密码是否足够复杂。
	/// </summary>
	/// <param name="password">密码</param>
	/// <returns>若密码足够复杂，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public static bool IsPasswordComplicated(string password) {
		if (password.Length < 10 || password.Length > 64) {
			return false;
		}
		var reg = RepeatRegex(); // 重复字符
		if (reg.IsMatch(password)) {
			return false;
		}
		var kind = 0;
		var kindComfirm = (Regex regex, ref int kind) => {
			var matches = regex.Matches(password);
			if (matches.Count > 1) { // 两个及以上视为有效种类
				kind++;
			}
		};
		kindComfirm(SymbleRegex(), ref kind); // 特殊字符
		kindComfirm(UpperLetterRegex(), ref kind); // 大写字母
		kindComfirm(LowerLetterRegex(), ref kind); // 小写字母
		kindComfirm(NumberRegex(), ref kind); // 数字
		return kind > 2; // 至少三种类型
	}

	/// <summary>
	/// 检查是否登录。
	/// </summary>
	/// <returns>若已登录，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public bool IsLogin() {
		// 写累了，等会再写。
		var a = false;
		return a;
	}



	[GeneratedRegex(@"[`~!@#$%^&*()_+=\[{\]};:'""<>|./\\?,\-]")]
	private static partial Regex SymbleRegex();
	[GeneratedRegex(@"[a-z]")]
	private static partial Regex LowerLetterRegex();
	[GeneratedRegex(@"[A-Z]")]
	private static partial Regex UpperLetterRegex();
	[GeneratedRegex(@"\d")]
	private static partial Regex NumberRegex();
	[GeneratedRegex(@"(?<a>.)\k<a>{3,}")]
	private static partial Regex RepeatRegex();
}