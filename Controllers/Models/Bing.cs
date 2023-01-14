namespace SimpleWebChatApplication.Controllers.Models;

public struct BingAPIRoot {
	public BingImageData[]? Images { get; set; }
}

public struct BingImageData {
	//public string Fullstartdate { get; set; }
	public string Url { get; set; }
	//public string Copyright { get; set; }
	//public string Copyrightlink { get; set; }
	//public string Title { get; set; }
}