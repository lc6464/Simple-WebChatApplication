using System.Security.Cryptography;

namespace SimpleWebChatApplication.Hubs;

internal class Group {
	private const int SaltLength = 16;
	private const int HashLength = 256;

	/// <summary>
	/// 群组成员数量
	/// </summary>
	public ulong MemberCount { get; set; } = 0;

	/// <summary>
	/// 群组名称
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// 群组密码的盐
	/// </summary>
	public byte[] PasswordSalt { get; private init; }

	/// <summary>
	/// 群组密码的哈希值
	/// </summary>
	public byte[] PasswordHash { get; private init; }

	/// <summary>
	/// 验证给定的密码是否与当前群组的密码相匹配。
	/// </summary>
	/// <param name="password">给定的密码</param>
	/// <returns>若匹配，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
	public bool VerifyPassword(string password) {
		var computeResult = new byte[HashLength];
		var passwordData = Encoding.Unicode.GetBytes(password);
		var combinedData = new byte[passwordData.Length + PasswordSalt.Length];
		Buffer.BlockCopy(passwordData, 0, combinedData, 0, passwordData.Length);
		Buffer.BlockCopy(PasswordSalt, 0, combinedData, passwordData.Length, PasswordSalt.Length);
		_ = SHA256.HashData(combinedData, computeResult);
		return PasswordHash.SequenceEqual(computeResult);
	}

	/// <summary>
	/// 创建一个新的群组。
	/// </summary>
	/// <param name="name">群组名称</param>
	/// <param name="password">群组密码</param>
	public Group(string name, string password) {
		Name = name;
		PasswordSalt = new byte[SaltLength];
		PasswordHash = new byte[HashLength];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(PasswordSalt);
		var passwordData = Encoding.Unicode.GetBytes(password);
		var combinedData = new byte[passwordData.Length + PasswordSalt.Length];
		Buffer.BlockCopy(passwordData, 0, combinedData, 0, passwordData.Length);
		Buffer.BlockCopy(PasswordSalt, 0, combinedData, passwordData.Length, PasswordSalt.Length);
		_ = SHA256.HashData(combinedData, PasswordHash);
	}
}