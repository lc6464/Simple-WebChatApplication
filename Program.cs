using SignalRWebpack.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();


var app = builder.Build();

app.MapHub<ChatHub>("/hub");

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();