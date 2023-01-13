namespace SignalRWebpack.Hubs;

internal struct Group {
	public string Name { get; set; }
	public byte[] PasswordSalt { get; set; }
	public byte[] PasswordHash { get; set; }
}