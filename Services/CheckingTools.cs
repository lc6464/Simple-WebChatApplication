using System.Security.Cryptography;
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
	/// 获取用户信息读取器。
	/// </summary>
	/// <param name="name">用户名</param>
	/// <returns>对应的 <see cref="SqliteDataReader"/>。</returns>
	public SqliteDataReader GetUserReader(string name) {
		using var transaction = Connection.BeginTransaction();
		using var cmd = Connection.CreateCommand();
		cmd.Transaction = transaction;
		cmd.CommandText = "SELECT * FROM Users WHERE Name = '@Name'";
		cmd.Parameters.AddWithValue("@Name", name);
		var reader = cmd.ExecuteReader();
		transaction.Commit();
		return reader;
	}

	/// <summary>
	/// 检查用户名是否可用。
	/// </summary>
	/// <param name="name">用户名</param>
	/// <returns>若可用则为 <see langword="false"/>，否则为 <see langword="true"/>。</returns>
	public bool IsNameAvailable(string name) => !(name.Length is < 4 or > 32 || !NameRegex().IsMatch(name) || GetUserReader(name).HasRows);

	/// <summary>
	/// 检查密码是否足够复杂。
	/// </summary>
	/// <param name="password">密码</param>
	/// <returns>若密码足够复杂且长度合适，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public static bool IsPasswordComplicated(string password) {
		if (password.Length is < 10 or > 64) {
			return false;
		}
		var reg = RepeatRegex(); // 重复字符
		if (reg.IsMatch(password)) {
			return false;
		}
		var kind = 0;
		KindComfirm(SymbleRegex(), ref kind, password); // 特殊字符
		KindComfirm(UpperLetterRegex(), ref kind, password); // 大写字母
		KindComfirm(LowerLetterRegex(), ref kind, password); // 小写字母
		KindComfirm(NumberRegex(), ref kind, password); // 数字
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

	public static ReadOnlySpan<byte> HashPassword(string password, out Span<byte> salt) {
		salt = new Span<byte>(new byte[16]);
		RandomNumberGenerator.Fill(salt);
		return HashPassword(password, salt);
	}

	public static ReadOnlySpan<byte> HashPassword(string password, ReadOnlySpan<byte> salt) {
		using HMACSHA512 sha512 = new(salt.ToArray());
		return sha512.ComputeHash(Encoding.UTF8.GetBytes(password));
	}

	public static bool ComfirmPassword(string password, ReadOnlySpan<byte> hash, ReadOnlySpan<byte> salt) => hash.SequenceEqual(HashPassword(password, salt));




	private static void KindComfirm(Regex regex, ref int kind, string password) {
		var matches = regex.Matches(password);
		if (matches.Count > 1) { // 两个及以上视为有效种类
			kind++;
		}
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

	[GeneratedRegex(@"^[A-Za-z][A-Za-z\d\-_]+$")]
	private static partial Regex NameRegex();
}