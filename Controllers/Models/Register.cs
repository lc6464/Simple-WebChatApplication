using System.Text.Json.Serialization;
using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Controllers.Models;

/// <summary>
/// RegisterController 用户 Post 方法的响应。
/// </summary>
public readonly struct RegisterUserPostResponse {
	public bool Success { get; init; }
	public int Code { get; init; }
	public string? Message { get; init; }
	public string? Data { get; init; }
}


/// <summary>
/// RegisterController 用户 Post 方法用户数据序列化模板。
/// </summary>
public readonly struct RegisterUserPostJsonSerializeTemplate {
	public RegisterUserPostJsonSerializeTemplate() => HMACKey = ICheckingTools.GenerateRandomData(16).ToArray();

	[JsonPropertyName("a")]
	public string? Account { get; init; }

	[JsonPropertyName("h")]
	public byte[]? PasswordHash { get; init; }

	[JsonPropertyName("s")]
	public byte[]? PasswordSalt { get; init; }

	[JsonPropertyName("k")]
	public byte[] HMACKey { get; init; }
}


/// <summary>
/// RegisterController 管理员 Get 方法的响应。
/// </summary>
public readonly struct RegisterGetResponse {
	public bool Success { get; init; }
	public int Code { get; init; }
	public string? Message { get; init; }
	public RegisterGetResponseUserData? Data { get; init; }
}


/// <summary>
/// RegisterController 管理员 Get 方法用户数据反序列化模板。
/// </summary>
public readonly struct RegisterGetJsonDeserializeTemplate {
	[JsonPropertyName("a")]
	public string? Account { get; init; }

	[JsonPropertyName("h")]
	public byte[]? PasswordHash { get; init; }

	[JsonPropertyName("s")]
	public byte[]? PasswordSalt { get; init; }

	[JsonPropertyName("k")]
	public byte[]? HMACKey { get; init; }
}


/// <summary>
/// RegisterController 管理员 Get 方法的响应中的用户数据。
/// </summary>
public readonly struct RegisterGetResponseUserData {
	public RegisterGetResponseUserData(RegisterGetJsonDeserializeTemplate userData, long timestamp) {
		Account = userData.Account;
		PasswordHash = userData.PasswordHash;
		PasswordSalt = userData.PasswordSalt;
		Timestamp = timestamp;
	}

	public string? Account { get; init; }

	public byte[]? PasswordHash { get; init; }

	public byte[]? PasswordSalt { get; init; }

	public long? Timestamp { get; init; }
}


/// <summary>
/// RegisterController Import 方法的响应。
/// </summary>
public readonly struct RegisterImportResponse {
	public bool Success { get; init; }
	public int Code { get; init; }
	public string? Message { get; init; }
	public string? Data { get; init; }
}