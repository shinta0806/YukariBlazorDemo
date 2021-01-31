// ============================================================================
// 
// 登録ユーザー関連データベースのコンテキスト
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
	public class UserProfileContext : DbContext
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// 登録ユーザーテーブル
		public DbSet<RegisteredUser>? RegisteredUsers { get; set; }

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
			modelBuilder.Entity<RegisteredUser>().HasIndex(x => x.Name).IsUnique();
			modelBuilder.Entity<HistorySong>().HasIndex(x => new { x.UserId, x.RequestTime }).IsUnique();
		}
	}
}
