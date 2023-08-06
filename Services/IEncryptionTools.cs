using System.Security.Cryptography;
using SimpleWebChatApplication.Controllers.Models;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 加密工具接口。
/// </summary>
public interface IEncryptionTools {
	/// <summary>
	/// AES256 加密。
	/// </summary>
	/// <param name="data">待加密的数据</param>
	/// <param name="key">加密密钥</param>
	/// <param name="iv">加密初始化向量</param>
	/// <param name="paddingMode">加密填充模式</param>
	/// <returns>加密结果</returns>
	public static byte[] Encrypt256(byte[] data, byte[] key, byte[] iv, PaddingMode paddingMode = PaddingMode.PKCS7) {
		using var aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		aes.Padding = paddingMode;

		using var encryptor = aes.CreateEncryptor();
		using MemoryStream ms = new();
		using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
		cs.Write(data, 0, data.Length);
		cs.FlushFinalBlock();
		return ms.ToArray();
	}

	/// <summary>
	/// AES256 加密一段文本。
	/// </summary>
	/// <param name="text">待加密的文本</param>
	/// <param name="key">加密密钥</param>
	/// <param name="iv">加密初始化向量</param>
	/// <param name="encoding">编码文本的编码</param>
	/// <param name="paddingMode">加密填充模式</param>
	/// <returns>加密结果</returns>
	public static byte[] Encrypt256(string text, byte[] key, byte[] iv, Encoding encoding, PaddingMode paddingMode = PaddingMode.PKCS7)
		=> Encrypt256(encoding.GetBytes(text), key, iv, paddingMode);

	/// <summary>
	/// AES256 加密一段文本，使用 UTF-8 编码文本。
	/// </summary>
	/// <param name="text">待加密的文本</param>
	/// <param name="key">加密密钥</param>
	/// <param name="iv">加密初始化向量</param>
	/// <param name="paddingMode">加密填充模式</param>
	/// <returns>加密结果</returns>
	public static byte[] Encrypt256(string text, byte[] key, byte[] iv, PaddingMode paddingMode = PaddingMode.PKCS7)
		=> Encrypt256(text, key, iv, Encoding.UTF8, paddingMode);

	/// <summary>
	/// AES256 加密，返回 Base64 编码的结果。
	/// </summary>
	/// <param name="data">待加密的数据</param>
	/// <param name="key">加密密钥</param>
	/// <param name="iv">加密初始化向量</param>
	/// <param name="paddingMode">加密填充模式</param>
	/// <returns>加密结果（Base64 编码）</returns>
	public static string Encrypt256Base64(byte[] data, byte[] key, byte[] iv, PaddingMode paddingMode = PaddingMode.PKCS7)
		=> Convert.ToBase64String(Encrypt256(data, key, iv, paddingMode));

	/// <summary>
	/// AES256 加密一段文本，返回 Base64 编码的结果。
	/// </summary>
	/// <param name="text">待加密的文本</param>
	/// <param name="key">加密密钥</param>
	/// <param name="iv">加密初始化向量</param>
	/// <param name="encoding">编码文本的编码</param>
	/// <param name="paddingMode">加密填充模式</param>
	/// <returns>加密结果（Base64 编码）</returns>
	public static string Encrypt256Base64(string text, byte[] key, byte[] iv, Encoding encoding, PaddingMode paddingMode = PaddingMode.PKCS7)
		=> Convert.ToBase64String(Encrypt256(text, key, iv, encoding, paddingMode));

	/// <summary>
	/// AES256 加密一段文本，使用 UTF-8 编码文本，返回 Base64 编码的结果。
	/// </summary>
	/// <param name="text">待加密的文本</param>
	/// <param name="key">加密密钥</param>
	/// <param name="iv">加密初始化向量</param>
	/// <param name="paddingMode">加密填充模式</param>
	/// <returns>加密结果（Base64 编码）</returns>
	public static string Encrypt256Base64(string text, byte[] key, byte[] iv, PaddingMode paddingMode = PaddingMode.PKCS7)
		=> Convert.ToBase64String(Encrypt256(text, key, iv, paddingMode));


	/// <summary>
	/// AES256 解密。
	/// </summary>
	/// <param name="encryptedData">加密数据</param>
	/// <param name="key">解密密钥</param>
	/// <param name="iv">解密初始化向量</param>
	/// <param name="paddingMode">解密填充模式</param>
	/// <returns>解密结果</returns>
	public static byte[] Decrypt256(byte[] encryptedData, byte[] key, byte[] iv, PaddingMode paddingMode = PaddingMode.PKCS7) {
		using var aes = Aes.Create();
		aes.Key = key;
		aes.IV = iv;
		aes.Padding = paddingMode;

		using var decryptor = aes.CreateDecryptor();
		using MemoryStream ms = new(encryptedData);
		using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
		using MemoryStream result = new();
		cs.CopyTo(result);
		return result.ToArray();
	}



	/// <summary>
	/// 加密 UserData。
	/// </summary>
	/// <param name="userData">待加密的 <see cref="RegisterUserData"/></param>
	/// <returns>加密后的结果</returns>
	public string EncryptUserData(RegisterUserData userData);

	/// <summary>
	/// 解密注册数据。
	/// </summary>
	/// <param name="input">待解密的字符串</param>
	/// <returns>解密后的数据</returns>
	public bool TryDecryptUserData(string[] input, out RegisterUserDataOutput? output);
}