namespace SimpleWebChatApplication.Controllers.Models;

public readonly struct Register {
	public bool Success { get; init; }
	public int Code { get; init; }
	public string? Message { get; init; }
	public string? Data { get; init; }
}

public readonly struct RegisterData {
	public string? Account { get; init; }
	public string? Password { get; init; }
	public string? RepeatPassword { get; init; }
}