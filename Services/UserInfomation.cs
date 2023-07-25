using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取用户信息的类。
/// </summary>
public class UserInfomation : IUserInfomation {
	/// <summary>
	/// 默认构造函数。
	/// </summary>
	public UserInfomation(IHostEnvironment hostEnvironment, IConfiguration configuration) {
		SqliteConnectionStringBuilder builder = new() {
			DataSource = Path.Combine(hostEnvironment.ContentRootPath, "users.db"),
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
		using var transaction = Connection.BeginTransaction();
		using var command = Connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = "";
		command.ExecuteNonQuery();
		transaction.Commit();
	}

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