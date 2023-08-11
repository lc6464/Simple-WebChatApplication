using System.Reflection;

namespace SimpleWebChatApplication.Controllers.Models;

public readonly struct Hello {
	public static readonly Assembly assembly = typeof(Hello).Assembly;
	public static readonly Version Version = assembly.GetName().Version!;

	public Hello(HttpContext context) => IP = new(context.Connection.RemoteIpAddress, context.Request.Protocol);

	public Hello(IHttpConnectionInfo info) => IP = new(info);

	public readonly DateTime Time => DateTime.UtcNow;

	public static readonly string Copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute))!).Copyright;

	public const string Text = "Welcome to Simple WebChat Application.";

	public IP IP { get; init; }
}