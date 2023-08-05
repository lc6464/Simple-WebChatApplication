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
	public readonly byte[] HMACKey = ICheckingTools.GenerateRandomData(16).ToArray();
}