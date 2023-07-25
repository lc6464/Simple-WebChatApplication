using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取当前后端配置信息的类。
/// </summary>
public class BackendConfiguration : IBackendConfiguration {
	/// <summary>
	/// 默认构造函数。
	/// </summary>
	public BackendConfiguration(IHostEnvironment hostEnvironment, IConfiguration configuration) {
		SqliteConnectionStringBuilder builder = new() {
			DataSource = Path.Combine(hostEnvironment.ContentRootPath, "backend.db"),
			Mode = (IsCreatedBefore = File.Exists(ConnectionString)) switch {
				true => SqliteOpenMode.ReadWrite,
				false => SqliteOpenMode.ReadWriteCreate
			}
		};
		if (configuration.GetValue<bool>("Database:UsingPassword")) {
			builder.Password = configuration.GetValue<string>("Database:Password");
		}
		ConnectionString = builder.ToString();
		Connection = new(ConnectionString);
		Connection.Open();
	}

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