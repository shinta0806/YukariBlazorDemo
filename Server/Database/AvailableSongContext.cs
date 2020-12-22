// ============================================================================
// 
// 予約可能曲データベースのコンテキスト
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Database
{
	public class AvailableSongContext : DbContext
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// テーブル
		public DbSet<AvailableSong>? AvailableSongs { get; set; }

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// データベース設定
		// --------------------------------------------------------------------
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			SqliteConnectionStringBuilder stringBuilder = new()
			{
				DataSource = "AvailableSongs.sqlite3"
			};
			optionsBuilder.UseSqlite(new SqliteConnection(stringBuilder.ToString()));
		}
	}
}
