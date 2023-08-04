using Microsoft.Data.Sqlite;

namespace SimpleWebChatApplication.Services;
/// <summary>
/// 获取数据库。
/// </summary>
public interface IDataProvider {
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
	/// 获取用户信息读取器。
	/// </summary>
	/// <param name="name">用户名</param>
	/// <returns>对应的 <see cref="SqliteDataReader"/>。</returns>
	public SqliteDataReader GetUserReader(string name);

	/// <summary>
	/// 在指定的事务中执行指定的命令。
	/// </summary>
	/// <param name="transaction">事物</param>
	/// <param name="commandText">命令</param>
	/// <returns>被改变的行数</returns>
	public int CmdExeNonQuery(SqliteTransaction transaction, string commandText);

	/// <summary>
	/// 在指定的事务中执行指定的命令。
	/// </summary>
	/// <param name="command">命令</param>
	/// <returns>被改变的行数</returns>
	public int CmdExeNonQuery(SqliteCommand command);

	/// <summary>
	/// 在指定的事务中执行指定的命令。
	/// </summary>
	/// <param name="transaction">事物</param>
	/// <param name="commandText">命令</param>
	/// <param name="command">创建的 <see cref="SqliteCommand"/> 实例</param>
	/// <returns>数据读取器。</returns>
	public SqliteDataReader CmdExeReader(SqliteTransaction transaction, string commandText, out SqliteCommand command);

	/// <summary>
	/// 在指定的事务中执行指定的命令。
	/// </summary>
	/// <param name="command">命令</param>
	/// <returns>数据读取器。</returns>
	public SqliteDataReader CmdExeReader(SqliteCommand command);

	/// <summary>
	/// 当前应用程序版本。
	/// </summary>
	public Version AppVersion => typeof(DataProvider).Assembly.GetName().Version!;
}