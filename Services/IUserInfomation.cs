using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取用户信息。
/// </summary>
public interface IUserInfomation {
	/// <summary>
	/// 获取用户数据库连接字符串。
	/// </summary>
	public string ConnectionString { get; init; }

	/// <summary>
	/// 获取用户数据库连接。
	/// </summary>
	public SqliteConnection Connection { get; init; }

	/// <summary>
	/// 获取数据库是否在过去已经被创建。
	/// </summary>
	public bool IsCreatedBefore { get; init; }
}