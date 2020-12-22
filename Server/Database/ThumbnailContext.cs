// ============================================================================
// 
// サムネイルデータベースのコンテキスト
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace YukariBlazorDemo.Server.Database
{
	public class ThumbnailContext : DbContext
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// テーブル
		public DbSet<Thumbnail>? Thumbnails { get; set; }

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
				DataSource = "Thumbnails.sqlite3"
			};
			optionsBuilder.UseSqlite(new SqliteConnection(stringBuilder.ToString()));
		}
	}
}
