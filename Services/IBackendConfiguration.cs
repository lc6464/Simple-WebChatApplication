using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取当前后端配置信息。
/// </summary>
public interface IBackendConfiguration {
	/// <summary>
	/// 获取当前的后端配置数据库连接字符串。
	/// </summary>
	public string ConnectionString { get; init; }

	/// <summary>
	/// 获取当前的后端配置数据库连接。
	/// </summary>
	public SqliteConnection Connection { get; init; }

	/// <summary>
	/// 获取数据库是否在曾经已经创建。
	/// </summary>
	public bool IsCreatedBefore { get; init; }
}