using LC6464.ASPNET.AddResponseHeaders;
using MessagePack;
using Microsoft.AspNetCore.Http.Connections;
using SimpleWebChatApplication.Hubs;
using SimpleWebChatApplication.Services;

if (args is ["install", _]) { // install <Password>
	Console.WriteLine("请保证你的密码强度足够，否则可能会被破解！");
	var password = args[1].Trim();
	if (!IGeneralTools.IsPasswordComplicated(password)) {
		Console.WriteLine("密码强度不足！");
		return;
	}
	var hash = IGeneralTools.HashPassword(password, out var salt);
	Console.WriteLine($"密码为：{password}");
	Console.WriteLine($"密码的哈希值为：{Convert.ToBase64String(hash)}");
	Console.WriteLine($"密码的盐值为：{Convert.ToBase64String(salt)}");
	Console.WriteLine("请将以上哈希值和盐值数据写入 appsettings.json，重启应用程序后生效。");
	Console.WriteLine("如果是第一次设置软件，建议将数据库密码一并更改，");
	Console.WriteLine("并添加一些联系方式。");
	return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddHttp304() // 添加 Http304 和 HttpConnectionInfo 服务
	.AddMemoryCache()
	.AddResponseCaching()
	.AddHttpsRedirection(options => {
		options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
		options.HttpsPort = builder.Configuration.GetValue<ushort>("Https:Port");
	}).AddHsts(options => {
		options.ExcludedHosts.Add("localhost");
		options.MaxAge = TimeSpan.FromDays(365);
	}).AddResponseCompression(options => {
		options.EnableForHttps = true;
		options.ExcludedMimeTypes = new[] { "application/json" }; // 这压缩不是浪费性能吗？没起太大作用
	}).AddControllers(options => {
		options.CacheProfiles.Add("Private30d", new() { Duration = 2592000, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Public30d", new() { Duration = 2592000, Location = ResponseCacheLocation.Any });
		options.CacheProfiles.Add("Private7d", new() { Duration = 604800, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Public7d", new() { Duration = 604800, Location = ResponseCacheLocation.Any });
		options.CacheProfiles.Add("Private1d", new() { Duration = 86400, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Public1d", new() { Duration = 86400, Location = ResponseCacheLocation.Any });
		options.CacheProfiles.Add("Private1h", new() { Duration = 3600, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Public1h", new() { Duration = 3600, Location = ResponseCacheLocation.Any });
		options.CacheProfiles.Add("Private10m", new() { Duration = 600, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Private5m", new() { Duration = 300, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Private1m", new() { Duration = 60, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("NoCache", new() { Duration = 0, Location = ResponseCacheLocation.None });
		options.CacheProfiles.Add("NoStore", new() { NoStore = true });
	});


builder.Services // 添加 SignalR 服务
	.AddSignalR(options => options.SupportedProtocols = new[] { "messagepack" })
	.AddMessagePackProtocol(options =>
		options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));

builder.Services.AddRazorPages();

builder.Services
	.AddDistributedMemoryCache()
	.AddSession(options => { // 添加 Session 服务
		options.IdleTimeout = TimeSpan.FromHours(1);
		options.Cookie.IsEssential = true;
		options.Cookie.Name = "Session";
	}).AddServicesInProject(); // 添加自定义服务


var app = builder.Build();

var usingUnsafeEval = "; script-src 'self' 'unsafe-eval'"; // 用于开发环境

if (!app.Environment.IsDevelopment()) {
	_ = app.UseExceptionHandler("/Error")
		.UseStatusCodePagesWithReExecute("/StatusCode/{0}");

	if (builder.Configuration.GetValue<bool>("Https:Use")) {
		_ = app.UseHttpsRedirection()
			.UseHsts();
	}

	usingUnsafeEval = "";
}

app.UseResponseCompression()
	.UseResponseCaching()
	.UseAddResponseHeaders(new HeaderDictionary {
		{ "X-Content-Type-Options", "nosniff" },
		{ "X-XSS-Protection", "1; mode=block" }
	}).UseAddResponseHeaders(builder.Configuration.GetValue<bool>("Https:Use")
	? new HeaderDictionary {
		{ "Expect-CT", "max-age=31536000; enforce" },
		{ "Content-Security-Policy", $"upgrade-insecure-requests; default-src 'self'; img-src 'self'; style-src 'self' 'unsafe-inline'; frame-ancestors 'self'{usingUnsafeEval}" }
	} : new HeaderDictionary {
		{ "Content-Security-Policy", $"default-src 'self'; img-src 'self'; style-src 'self' 'unsafe-inline'; frame-ancestors 'self'{usingUnsafeEval}" }
	})
	.UseDefaultFiles()
	.UseStaticFiles(new StaticFileOptions {
		OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = app.Environment.IsDevelopment() ? "no-cache" : "public,max-age=1209600" // 14天
	});


app.UseSession();


app.MapRazorPages();

app.MapControllers();

app.MapHub<ChatHub>("/chathub", options => options.Transports = HttpTransportType.WebSockets);

app.Run();