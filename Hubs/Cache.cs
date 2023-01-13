namespace SignalRWebpack.Hubs;

internal static class Cache {
	public static IMemoryCache MemoryCache { get; } = new MemoryCache(new MemoryCacheOptions());

	public static T Set<T>(object key, T value, TimeSpan slidingExpiration) {
		using var entry = MemoryCache.CreateEntry(key);
		entry.SetSlidingExpiration(slidingExpiration);
		entry.Value = value;
		return value;
	}
}