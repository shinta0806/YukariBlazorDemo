// ============================================================================
// 
// 予約された曲データベースのコンテキスト
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
	public class RequestSongContext : DbContext
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// テーブル
		public DbSet<RequestSong>? RequestSongs { get; set; }

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
				DataSource = "RequestSongs.sqlite3"
			};
			optionsBuilder.UseSqlite(new SqliteConnection(stringBuilder.ToString()));
		}
	}
}
