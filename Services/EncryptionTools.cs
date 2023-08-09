using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using SimpleWebChatApplication.Controllers.Models;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 加密工具类。
/// </summary>
public class EncryptionTools : IEncryptionTools {
	private readonly IDataProvider _provider;

	public EncryptionTools(IDataProvider provider) => _provider = provider;


	/// <summary>
	/// 加密 UserData。
	/// </summary>
	/// <param name="userData">待加密的 <see cref="RegisterUserPostJsonSerializeTemplate"/></param>
	/// <returns>加密后的结果</returns>
	public string EncryptUserData(RegisterUserPostJsonSerializeTemplate userData) {
		var utf8UserData = JsonSerializer.SerializeToUtf8Bytes(userData); // 序列化 UserData
		var timestamp = BitConverter.GetBytes(ICheckingTools.Timestamp); // 获取当前时间戳数据

		var signRawData = new byte[timestamp.Length + utf8UserData.Length]; // 创建签名前的原始数据
		Buffer.BlockCopy(timestamp, 0, signRawData, 0, timestamp.Length); // 将时间戳数据复制到原始数据中
		Buffer.BlockCopy(utf8UserData, 0, signRawData, timestamp.Length, utf8UserData.Length); // 将 UserData Utf8Bytes 复制到原始数据中

		using HMACSHA256 sha256 = new(userData.HMACKey); // 创建 HMACSHA256 对象，Key 随机生成
		var signData = sha256.ComputeHash(signRawData); // 计算签名数据

		var signatureRawData = new byte[signData.Length + timestamp.Length]; // 创建签名数据
		Buffer.BlockCopy(signData, 0, signatureRawData, 0, signData.Length); // 将签名结果复制到签名数据中
		Buffer.BlockCopy(timestamp, 0, signatureRawData, signData.Length, timestamp.Length); // 将时间戳数据复制到签名数据中

		var encryptedDataString = Base64UrlTextEncoder.Encode(IEncryptionTools.Encrypt256(utf8UserData, _provider.RegisterEncryptionKey, _provider.RegisterEncryptionIV));
		var signature = Base64UrlTextEncoder.Encode(signatureRawData);
		return $"{encryptedDataString}/{signature}"; // 返回加密后的数据
	}


	/// <summary>
	/// 解密注册数据。
	/// </summary>
	/// <param name="input">待解密的字符串</param>
	/// <param name="output">输出的 <see cref="RegisterGetResponseUserData"/></param>
	/// <returns>解密后的数据</returns>
	public bool TryDecryptUserData(string[] input, out RegisterGetResponseUserData? output) {
		var signaturePart = Base64UrlTextEncoder.Decode(input[1]); // 签名部分
		var signature = signaturePart[..^8]; // 签名
		var timestamp = signaturePart[^8..]; // 时间戳

		var utf8UserData = IEncryptionTools.Decrypt256(Base64UrlTextEncoder.Decode(input[0]),
			_provider.RegisterEncryptionKey, _provider.RegisterEncryptionIV); // 解密后的 UserData

		var userData = JsonSerializer.Deserialize<RegisterGetJsonDeserializeTemplate>(utf8UserData); // 反序列化 UserData

		var signRawData = new byte[timestamp.Length + utf8UserData.Length]; // 创建签名前的原始数据
		Buffer.BlockCopy(timestamp, 0, signRawData, 0, timestamp.Length); // 将时间戳数据复制到原始数据中
		Buffer.BlockCopy(utf8UserData, 0, signRawData, timestamp.Length, utf8UserData.Length); // 将 UserData Utf8Bytes 复制到原始数据中
		using HMACSHA256 sha256 = new(userData.HMACKey!); // 使用传入的 Key 创建 HMACSHA256 对象
		var signData = sha256.ComputeHash(signRawData); // 计算签名数据

		if (signData.SequenceEqual(signature)) { // 比较签名数据
			output = new(userData, BitConverter.ToInt64(timestamp)); // 返回解密后的数据
			return true;
		}
		output = null;
		return false;
	}
}