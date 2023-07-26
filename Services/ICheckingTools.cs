namespace SimpleWebChatApplication.Services;
/// <summary>
/// 一些常用的检查工具。
/// </summary>
public interface ICheckingTools {
	/// <summary>
	/// 检查用户名是否可用。
	/// </summary>
	/// <param name="name">用户名</param>
	/// <returns>若被使用或长度不合适，则为 <see langword="false"/>，若可用则为 <see langword="true"/>。</returns>
	public bool IsNameAvailable(string name);

	/// <summary>
	/// 检查密码是否足够复杂。
	/// </summary>
	/// <param name="password">密码</param>
	/// <returns>若密码足够复杂，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public static abstract bool IsPasswordComplicated(string password);

	/// <summary>
	/// 检查是否登录。
	/// </summary>
	/// <returns>若已登录，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
	public bool IsLogin();
}