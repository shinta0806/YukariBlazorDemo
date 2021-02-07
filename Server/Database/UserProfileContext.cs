// ============================================================================
// 
// 登録ユーザー関連データベースのコンテキスト
// 
// ============================================================================

// ----------------------------------------------------------------------------
// StockSong と HistorySong は本来同じクラスで構わない
// 同じクラスにした時にテーブル名や制約条件を分ける方法が分からないためクラスを分けている
// ----------------------------------------------------------------------------

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Database;

namespace YukariBlazorDemo.Server.Database
{
	public class UserProfileContext : DbContext
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 登録ユーザーテーブル
		public DbSet<RegisteredUser>? RegisteredUsers { get; set; }

		// 登録ユーザーの後で歌う予定テーブル
		public DbSet<StockSong>? StockSongs { get; set; }

		// 登録ユーザーのマイ履歴テーブル
		public DbSet<HistorySong>? HistorySongs { get; set; }

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
				DataSource = ServerConstants.FILE_NAME_USER_PROFILES,
			};
			optionsBuilder.UseSqlite(new SqliteConnection(stringBuilder.ToString()));
		}

		// --------------------------------------------------------------------
		// データベース作成
		// --------------------------------------------------------------------
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// 同じ名前のユーザーは許容しない
			modelBuilder.Entity<RegisteredUser>().HasIndex(x => x.Name).IsUnique();

			// 同一ユーザーが複数回同じ曲を後で歌う予定に登録することは許容しない
			modelBuilder.Entity<StockSong>().HasIndex(x => new { x.UserId, x.AvailableSongId }).IsUnique();

			// 同一ユーザーが同時に予約することは無いはず
			modelBuilder.Entity<HistorySong>().HasIndex(x => new { x.UserId, x.RequestTime }).IsUnique();
		}
	}
}
