using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取数据库的类。
/// </summary>
public partial class DataProvider : IDataProvider {
	private readonly ILogger<DataProvider> _logger;

	/// <summary>
	/// 初始化应用程序信息。
	/// </summary>
	/// <param name="key">数据键</param>
	/// <param name="initData">要初始化的数据值</param>
	/// <param name="existData">已有的数据值</param>
	/// <returns>若原数据已存在，则 <paramref name="key"/> 无意义，<paramref name="existData"/> 被输出，且返回 true；若元数据不存在，则写入 <paramref name="key"/>，并返回 false。</returns>
	private bool InitAppInfo(SqliteTransaction transaction, string key, ReadOnlySpan<byte> initData, out ReadOnlySpan<byte> existData) {
		using var cmd = Connection.CreateCommand();
		cmd.Transaction = transaction;
		cmd.CommandText = "Select Value, Length from AppInfo where Key = @Key";
		cmd.Parameters.AddWithValue("@Key", key);
		using var reader = cmd.ExecuteReader();
		// 处理版本
		if (reader.Read()) {
			var dataLength = reader.GetInt32(1);
			var data = new byte[dataLength];
			_ = reader.GetBytes(0, 0, data, 0, dataLength);
			existData = data;
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
			_ = cmd.Parameters.AddWithValue("Version", data);
			_ = cmd.Parameters.AddWithValue("Length", data.Length);
			_ = cmd.ExecuteNonQuery();
			logger.LogInformation("数据库初始化成功！");
		}
		readerCmd.Dispose();
	}


	/// <summary>
	/// 默认构造函数。
	/// </summary>
	public DataProvider(IHostEnvironment hostEnvironment, IConfiguration configuration, ILogger<DataProvider> logger) {
		_logger = logger;
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
		_ = CmdExeNonQuery(transaction, "Create Table if not exists Users (ID integer primary key autoincrement, Name varchar(32) unique not null, Nick varchar(32), Hash blob not null, Salt blob not null)");
		_ = CmdExeNonQuery(transaction, "Create Table if not exists AppInfo (ID integer primary key autoincrement, Key varchar(128) unique not null, Value blob, Length integer not null)");
		
		transaction.Commit();
	}


	/// <summary>
	/// 获取用户信息读取器。
	/// </summary>
	/// <param name="name">用户名</param>
	/// <returns>对应的 <see cref="SqliteDataReader"/>。</returns>
	public SqliteDataReader GetUserReader(string name) {
		using var transaction = Connection.BeginTransaction();
		using var cmd = Connection.CreateCommand();
		cmd.Transaction = transaction;
		cmd.CommandText = "SELECT * FROM Users WHERE Name = '@Name'";
		_ = cmd.Parameters.AddWithValue("@Name", name);
		var reader = cmd.ExecuteReader();
		transaction.Commit();
		return reader;
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
	/// <param name="command">创建的 <see cref="SqliteCommand"/> 实例</param>
	/// <returns>数据读取器。</returns>
	public SqliteDataReader CmdExeReader(SqliteTransaction transaction, string commandText, out SqliteCommand command) {
		command = Connection.CreateCommand();
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


	/// <summary>
	/// 当前应用程序版本。
	/// </summary>
	public Version AppVersion => Controllers.Models.Hello.Version;
}