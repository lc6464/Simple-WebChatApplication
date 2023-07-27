using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 一些常用的检查工具。
/// </summary>
public partial interface ICheckingTools {
	/// <summary>
	/// 检查是否登录。
	/// </summary>
	/// <returns>若已登录，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public bool IsLogin();

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
		KindComfirm(SymbleRegex(), ref kind, password); // 特殊字符
		KindComfirm(UpperLetterRegex(), ref kind, password); // 大写字母
		KindComfirm(LowerLetterRegex(), ref kind, password); // 小写字母
		KindComfirm(NumberRegex(), ref kind, password); // 数字
		return kind > 2; // 至少三种类型
	}

	/// <summary>
	/// 计算给定密码的哈希值，盐随机生成。
	/// </summary>
	/// <param name="password">密码</param>
	/// <param name="salt">生成的盐值</param>
	/// <returns>密码的哈希</returns>
	public static ReadOnlySpan<byte> HashPassword(string password, out Span<byte> salt) {
		salt = new Span<byte>(new byte[16]);
		RandomNumberGenerator.Fill(salt);
		return HashPassword(password, salt);
	}

	/// <summary>
	/// 计算给定密码的哈希值。
	/// </summary>
	/// <param name="password">密码</param>
	/// <param name="salt">盐值</param>
	/// <returns>密码的哈希</returns>
	public static ReadOnlySpan<byte> HashPassword(string password, ReadOnlySpan<byte> salt) {
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

	[GeneratedRegex(@"(?<a>.)\k<a>{3}")]
	private static partial Regex RepeatRegex();
}