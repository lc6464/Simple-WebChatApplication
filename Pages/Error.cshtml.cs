using System.Diagnostics;

namespace SimpleWebChatApplication.Pages;

[ResponseCache(NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel {
	public string? RequestId { get; set; }

	public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

	private readonly ILogger<ErrorModel> _logger;

	public ErrorModel(ILogger<ErrorModel> logger) => _logger = logger;

	public void OnGet() {
		RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
		ViewData["Title"] = "错误";
		ViewData["Keywords"] = "LC,网站,测试,错误";
		ViewData["Description"] = "LC的测试站的错误页。";
	}
}