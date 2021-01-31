// ============================================================================
// 
// サムネイルデータベースのコンテキスト
// 
// ============================================================================

// ----------------------------------------------------------------------------
// デモなので読み書き両方を行うが、実際の運用では本アプリは読み込みのみとなる想定
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
