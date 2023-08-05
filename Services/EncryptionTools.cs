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
	//private readonly ICheckingTools _tools;

	public EncryptionTools(IDataProvider provider) => _provider = provider;

	/*
	public EncryptionTools(IDataProvider provider, ICheckingTools tools) {
		_provider = provider;
		_tools = tools;
	}*/


	/// <summary>
	/// 加密 UserData。
	/// </summary>
	/// <param name="userData">待加密的 <see cref="RegisterUserData"/></param>
	/// <returns>加密后的结果</returns>
	public string EncryptUserData(RegisterUserData userData) {
		var data = JsonSerializer.SerializeToUtf8Bytes(userData); // 序列化 UserData
		var timestamp = BitConverter.GetBytes(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()); // 获取当前时间戳数据
		var signRawData = new byte[timestamp.Length + data.Length]; // 创建签名前的原始数据
		Buffer.BlockCopy(timestamp, 0, signRawData, 0, timestamp.Length); // 将时间戳数据复制到原始数据中
		Buffer.BlockCopy(data, 0, signRawData, timestamp.Length, data.Length); // 将 UserData Utf8Bytes 复制到原始数据中
		using HMACSHA256 sha256 = new(userData.HMACKey); // 创建 HMACSHA256 对象，Key 随机生成
		var signData = sha256.ComputeHash(signRawData); // 计算签名数据
		var signatureRawData = new byte[signData.Length + timestamp.Length]; // 创建签名数据
		Buffer.BlockCopy(signData, 0, signatureRawData, 0, signData.Length); // 将签名结果复制到签名数据中
		Buffer.BlockCopy(timestamp, 0, signatureRawData, signData.Length, timestamp.Length); // 将时间戳数据复制到签名数据中
		var encryptedDataString = Base64UrlTextEncoder.Encode(IEncryptionTools.Encrypt256(data, _provider.RegisterEncryptionKey, _provider.RegisterEncryptionIV));
		var signature = Base64UrlTextEncoder.Encode(signatureRawData);
		return $"{encryptedDataString}/{signature}"; // 返回加密后的数据
	}
}