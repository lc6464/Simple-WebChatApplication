using Microsoft.Data.Sqlite;
namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取数据库的类。
/// </summary>
public class DataProvider : IDataProvider {
	/// <summary>
	/// 默认构造函数。
	/// </summary>
	public DataProvider(IHostEnvironment hostEnvironment, IConfiguration configuration, ILogger<DataProvider> logger) {
		// 构建连接字符串
		SqliteConnectionStringBuilder builder = new() {
			DataSource = Path.Combine(hostEnvironment.ContentRootPath, "data.db"),
			Mode = (IsCreatedBefore = File.Exists(ConnectionString)) switch {
				true => SqliteOpenMode.ReadWrite,
				false => SqliteOpenMode.ReadWriteCreate
			}
		};
		if (configuration.GetValue<bool>("Database:UsingPassword")) {
			builder.Password = configuration.GetValue<string>("Database:Password");
		}
		// 连接数据库并创建数据表
		ConnectionString = builder.ToString();
		Connection = new(ConnectionString);
		Connection.Open();
		using var transaction = Connection.BeginTransaction();
		CmdExeNonQuery(transaction, "Create Table if not exists Users (ID integer primary key autoincrement, Name varchar(32) unique not null, Nick varchar(32), Hash blob not null, Salt blob not null)");
		CmdExeNonQuery(transaction, "Create Table if not exists AppInfo (ID integer primary key autoincrement, Key varchar(128) unique not null, Value blob, Length integer not null)");
		using var reader = CmdExeReader(transaction, "Select Value from AppInfo where Key = 'Version'");
		// 处理版本
		if (reader.Read()) {
			var dataLength = reader.GetInt32(1);
			var data = new byte[dataLength];
			reader.GetBytes(0, 0, data, 0, dataLength);
			Version version = new(Encoding.UTF8.GetString(data));
			var result = AppVersion.CompareTo(version);
			if (result < 0) {
				logger.LogWarning("此应用程序的版本高于数据库中的版本，有可能导致应用程序无法正常运行，请特别留意！");
			} else if (result > 0) {
				logger.LogWarning("此应用程序已完成升级，请留意开源项目地址是否有更新数据库相关说明。");
			} else {
				logger.LogInformation("数据库已就绪。");
			}
		} else {
			// 写入版本
			using var cmd = Connection.CreateCommand();
			cmd.Transaction = transaction;
			cmd.CommandText = "Insert into AppInfo (Key, Value, Length) values ('Version', @Version, @Length)";
			var data = Encoding.UTF8.GetBytes(AppVersion.ToString());
			cmd.Parameters.AddWithValue("Version", data);
			cmd.Parameters.AddWithValue("Length", data.Length);
			cmd.ExecuteNonQuery();
			logger.LogInformation("数据库初始化成功！");
		}
		transaction.Commit();
	}

	/// <summary>
	/// 在指定的事务中执行指定的命令。
	/// </summary>
	/// <param name="transaction">事物</param>
	/// <param name="commandText">命令</param>
	/// <returns>被改变的行数</returns>
	public int CmdExeNonQuery(SqliteTransaction transaction, string commandText) {
		using var command = Connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = commandText;
		return command.ExecuteNonQuery();
	}

	/// <summary>
	/// 在指定的事务中执行指定的命令。
	/// </summary>
	/// <param name="command">命令</param>
	/// <returns>被改变的行数</returns>
	public int CmdExeNonQuery(SqliteCommand command) {
		var result = command.ExecuteNonQuery();
		command.Dispose();
		return result;
	}

	/// <summary>
	/// 在指定的事务中执行指定的命令。
	/// </summary>
	/// <param name="transaction">事物</param>
	/// <param name="commandText">命令</param>
	/// <returns>数据读取器。</returns>
	public SqliteDataReader CmdExeReader(SqliteTransaction transaction, string commandText) {
		using var command = Connection.CreateCommand();
		command.Transaction = transaction;
		command.CommandText = commandText;
		return command.ExecuteReader();
	}

	/// <summary>
	/// 在指定的事务中执行指定的命令。
	/// </summary>
	/// <param name="command">命令</param>
	/// <returns>数据读取器。</returns>
	public SqliteDataReader CmdExeReader(SqliteCommand command) {
		using var reader = command.ExecuteReader();
		command.Dispose();
		return reader;
	}

	/// <summary>
	/// 获取数据库连接字符串。
	/// </summary>
	public string ConnectionString { get; init; }

	/// <summary>
	/// 获取数据库连接。
	/// </summary>
	public SqliteConnection Connection { get; init; }

	/// <summary>
	/// 获取数据库是否在过去已经被创建。
	/// </summary>
	public bool IsCreatedBefore { get; init; }

	/// <summary>
	/// 当前应用程序版本。
	/// </summary>
	public Version AppVersion => Controllers.Models.Hello.Version;
}