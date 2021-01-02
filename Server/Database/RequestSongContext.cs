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

using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Database;

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
				DataSource = ServerConstants.FILE_NAME_REQUEST_SONGS,
			};
			optionsBuilder.UseSqlite(new SqliteConnection(stringBuilder.ToString()));
		}
	}
}
