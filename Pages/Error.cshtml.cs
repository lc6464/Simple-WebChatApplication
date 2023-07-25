using System.Diagnostics;

namespace SimpleWebChatApplication.Pages;

[ResponseCache(NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel {
	public string? RequestId { get; set; }

	public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

	//private readonly ILogger<ErrorModel> _logger;

	public ErrorModel(/*ILogger<ErrorModel> logger*/) { }//=> _logger = logger;

	public void OnGet() {
		RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
		ViewData["Title"] = "错误";
		ViewData["Keywords"] = "错误";
		ViewData["Description"] = "此应用程序在运行过程中发生了错误，请联系站长处理。";
	}
}