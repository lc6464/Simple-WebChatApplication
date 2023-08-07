using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 检查工具接口。
/// </summary>
public partial interface ICheckingTools {
	/// <summary>
	/// 检查用户名是否可用。
	/// </summary>
	/// <param name="name">用户名</param>
	/// <returns>若被使用或长度不合适，则为 <see langword="false"/>，若可用则为 <see langword="true"/>。</returns>
	public bool IsNameAvailable(string name);

	/// <summary>
	/// 检查是否登录。
	/// </summary>
	/// <returns>若已登录，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public bool IsLogin();

	/// <summary>
	/// 检查是否登录。
	/// </summary>
	/// <param name="displayName">输出显示的用户名。</param>
	/// <returns>若已登录，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public bool IsLogin(out string displayName);


	/// <summary>
	/// 检查密码是否足够复杂。
	/// </summary>
	/// <param name="password">密码</param>
	/// <returns>若密码足够复杂，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public static bool IsPasswordComplicated(string password) {
		if (password.Length is < 10 or > 64) {
			return false;
		}
		var reg = RepeatRegex(); // 重复字符
		if (reg.IsMatch(password)) {
			return false;
		}
		var kind = 0;
		KindConfirm(SymbolRegex(), ref kind, password); // 特殊字符
		KindConfirm(UpperLetterRegex(), ref kind, password); // 大写字母
		KindConfirm(LowerLetterRegex(), ref kind, password); // 小写字母
		KindConfirm(NumberRegex(), ref kind, password); // 数字
		return kind > 2; // 至少三种类型
	}

	/// <summary>
	/// 生成随机数据。
	/// </summary>
	/// <param name="length">数据长度</param>
	/// <returns>生成的随机数据</returns>
	public static ReadOnlySpan<byte> GenerateRandomData(int length) {
		Span<byte> data = new byte[length];
		RandomNumberGenerator.Fill(data);
		return data;
	}

	/// <summary>
	/// 生成随机的16字节盐值。
	/// </summary>
	/// <returns>随机的16字节盐。</returns>
	public static ReadOnlySpan<byte> GenerateSalt() => GenerateRandomData(16);

	/// <summary>
	/// 计算给定密码的哈希值，盐随机生成。
	/// </summary>
	/// <param name="password">密码</param>
	/// <param name="salt">生成的盐值</param>
	/// <returns>密码的哈希</returns>
	public static ReadOnlySpan<byte> HashPassword(string password, out ReadOnlySpan<byte> salt) {
		salt = GenerateSalt();
		return HashPassword(password, salt);
	}

	/// <summary>
	/// 计算给定密码的哈希值。
	/// </summary>
	/// <param name="password">密码</param>
	/// <param name="salt">盐值</param>
	/// <returns>密码的哈希</returns>
	public static ReadOnlySpan<byte> HashPassword(string password, ReadOnlySpan<byte> salt) {
		if (salt.Length != 16) {
			throw new ArgumentException("The length of salt must be 16 in this project.", nameof(salt));
		}
		using HMACSHA512 sha512 = new(salt.ToArray());
		return sha512.ComputeHash(Encoding.UTF8.GetBytes(password));
	}

	/// <summary>
	/// 检查密码是否正确。
	/// </summary>
	/// <param name="password">密码</param>
	/// <param name="hash">正确密码的哈希</param>
	/// <param name="salt">对应的盐值</param>
	/// <returns>若正确，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public static bool VerifyPassword(string password, ReadOnlySpan<byte> hash, ReadOnlySpan<byte> salt) => hash.SequenceEqual(HashPassword(password, salt));

	private static void KindConfirm(Regex regex, ref int kind, string password) {
		var matches = regex.Matches(password);
		if (matches.Count > 1) { // 两个及以上视为有效种类
			kind++;
		}
	}


	[GeneratedRegex(@"[ `~!@#$%^&*()_+=\[{\]};:'""<>|./\\?,\-]")]
	private static partial Regex SymbolRegex();

	[GeneratedRegex(@"[a-z]")]
	private static partial Regex LowerLetterRegex();

	[GeneratedRegex(@"[A-Z]")]
	private static partial Regex UpperLetterRegex();

	[GeneratedRegex(@"\d")]
	private static partial Regex NumberRegex();

	[GeneratedRegex(@"(?<a>.)\k<a>{3}")]
	private static partial Regex RepeatRegex();
}