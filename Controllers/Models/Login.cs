namespace SimpleWebChatApplication.Controllers.Models;

public readonly struct Login {
	public bool Success { get; init; }
	public int Code { get; init; }
	public string? Message { get; init; }
}