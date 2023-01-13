using MessagePack;
using SignalRWebpack.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR().AddMessagePackProtocol(options =>
	options.SerializerOptions = MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData));


var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<ChatHub>("/hub");

app.Run();