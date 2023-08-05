using System.Text.Json.Serialization;
using SimpleWebChatApplication.Services;

namespace SimpleWebChatApplication.Controllers.Models;

public readonly struct Register {
	public bool Success { get; init; }
	public int Code { get; init; }
	public string? Message { get; init; }
	public string? Data { get; init; }
}

public readonly struct RegisterUserData {
	public RegisterUserData() { }

	[JsonPropertyName("a")]
	public string? Account { get; init; }

	[JsonPropertyName("h")]
	public byte[]? PasswordHash { get; init; }

	[JsonPropertyName("s")]
	public byte[]? PasswordSalt { get; init; }

	[JsonPropertyName("k")]
	public byte[] HMACKey => ICheckingTools.GenerateRandomData(16).ToArray();
}



public readonly struct RegisterGet {
	public bool Success { get; init; }
	public int Code { get; init; }
	public string? Message { get; init; }
	public RegisterUserDataOutput? Data { get; init; }
}

public readonly struct RegisterUserDataGet {
	[JsonPropertyName("a")]
	public string? Account { get; init; }

	[JsonPropertyName("h")]
	public byte[]? PasswordHash { get; init; }

	[JsonPropertyName("s")]
	public byte[]? PasswordSalt { get; init; }

	[JsonPropertyName("k")]
	public byte[]? HMACKey { get; init; }
}

public readonly struct RegisterUserDataOutput {
	public RegisterUserDataOutput(RegisterUserDataGet userData, long timestamp) {
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