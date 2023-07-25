namespace SimpleWebChatApplication.Services;
/// <summary>
/// 为 <see cref="IServiceCollection">builder.Services</see> 添加扩展方法。
/// </summary>
public static class IServiceCollectionExtensions {
	/// <summary>
	/// 为 <paramref name="services"/> 添加 <see cref="IUserInfomation"/> 的扩展方法。
	/// </summary>
	/// <param name="services"><see cref="IServiceCollection">builder.Services</see></param>
	/// <returns>当前的 <see cref="IServiceCollection"/>，用于链式调用。</returns>
	public static IServiceCollection AddConfiguration(this IServiceCollection services) =>
		services.AddSingleton<IUserInfomation, UserInfomation>();
}