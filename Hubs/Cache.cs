namespace SimpleWebChatApplication.Hubs;

internal static class Cache {
	public static IMemoryCache MemoryCache { get; } = new MemoryCache(new MemoryCacheOptions());

	public static T Set<T>(object key, T value, TimeSpan slidingExpiration) {
		using var entry = MemoryCache.CreateEntry(key);
		_ = entry.SetSlidingExpiration(slidingExpiration);
		entry.Value = value;
		return value;
	}
	public static T Set<T>(object key, T value, TimeSpan slidingExpiration, TimeSpan absoluteExpiration) {
		using var entry = MemoryCache.CreateEntry(key);
		_ = entry.SetSlidingExpiration(slidingExpiration);
		_ = entry.SetAbsoluteExpiration(absoluteExpiration);
		entry.Value = value;
		return value;
	}
}