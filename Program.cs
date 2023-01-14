using LC6464.ASPNET.AddResponseHeaders;
using MessagePack;
using SimpleWebChatApplication.Hubs;

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
		options.IncludeSubDomains = true;
		options.MaxAge = TimeSpan.FromDays(365);
		options.Preload = true;
	}).AddResponseCompression(options => {
		options.EnableForHttps = true;
		options.ExcludedMimeTypes = new[] { "application/json" }; // 这压缩不是浪费性能吗？没起太大作用
	}).AddControllers(options => {
		options.CacheProfiles.Add("Private30d", new() { Duration = 2592000, Location = ResponseCacheLocation.Client });
		options.CacheProfiles.Add("Public30d", new() { Duration = 2592000, Location = ResponseCacheLocation.Any });
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
	.AddSignalR()
	.AddMessagePackProtocol(options =>
		options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));

builder.Services.AddRazorPages();


var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Error")
		.UseStatusCodePagesWithReExecute("/StatusCode/{0}");
	
	if (builder.Configuration.GetValue<bool>("Https:Use")) {
		_ = app.UseHttpsRedirection()
		.UseHsts();
	}
	
}

app.UseResponseCompression()
	.UseCors()
	.UseResponseCaching()
	.UseAddResponseHeaders(new HeaderDictionary {
		{ "X-Content-Type-Options", "nosniff" },
		{ "X-XSS-Protection", "1; mode=block" }
	}).UseAddResponseHeaders(builder.Configuration.GetValue<bool>("Https:Use")
	? new HeaderDictionary {
		{ "Expect-CT", "max-age=31536000; enforce" },
		{ "Content-Security-Policy", "upgrade-insecure-requests; default-src 'self'; img-src 'self' https://*.bing.com/th; frame-ancestors 'self'" }
	} : new HeaderDictionary {
		{ "Content-Security-Policy", "default-src 'self'; img-src 'self' https://*.bing.com/th; frame-ancestors 'self'" }
	})
	.UseDefaultFiles()
	.UseStaticFiles(new StaticFileOptions {
		OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "public,max-age=2592000" // 30天
	});


app.MapRazorPages();

app.MapControllers();

app.MapHub<ChatHub>("/chat");

app.Run();