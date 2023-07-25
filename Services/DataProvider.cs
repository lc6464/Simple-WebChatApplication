using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取数据库的类。
/// </summary>
public class DataProvider : IDataProvider {
	/// <summary>
	/// 默认构造函数。
	/// </summary>
	public DataProvider(IHostEnvironment hostEnvironment, IConfiguration configuration) {
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
		ConnectionString = builder.ToString();
		Connection = new(ConnectionString);
		Connection.Open();
		using var transaction = Connection.BeginTransaction();
		CmdExeNonQuery(transaction, "Create Table if not exists User (ID integer primary key autoincrement, Name varchar(32) unique not null, Nick varchar(32), Hash blob not null, Salt blob not null)");
		CmdExeNonQuery(transaction, "Create Table if not exists AppInfo (ID integer primary key autoincrement, Key varchar(128) unique not null, Value blob, Length integer not null)");
		var reader = CmdExeReader(transaction, "Select Value from AppInfo where Key = 'Version'");

		var nowVersion = typeof(DataProvider).Assembly.GetName().Version!;

		if (reader.Read()) {
			var dataLength = reader.GetInt32(1);
			var data = new byte[dataLength];
			reader.GetBytes(0, 0, data, 0, dataLength);
			Version version = new(Encoding.UTF8.GetString(data));
			var result = nowVersion.CompareTo(version);
			if (result < 0) {
				Console.WriteLine("此应用程序可能遭遇过降级，抑或是数据库发生过错误，有可能导致应用程序无法正常运行，请特别留意！");
			} else if (result > 0) {
				Console.WriteLine("此应用程序已完成升级，请留意开源项目地址是否有更新数据库相关说明。");
			}
		} else {
			using var cmd = Connection.CreateCommand();
			cmd.Transaction = transaction;
			cmd.CommandText = "Insert into AppInfo (Key, Value) values ('Version', @Version)";
			cmd.Parameters.AddWithValue("Version", Encoding.UTF8.GetBytes(nowVersion.ToString()));
			cmd.ExecuteNonQuery();
			Console.WriteLine("数据库初始化成功！");
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
		var reader = command.ExecuteReader();
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
}