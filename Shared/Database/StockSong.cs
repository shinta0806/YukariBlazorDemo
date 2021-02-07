// ============================================================================
// 
// 後で歌う予定に登録された曲の情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukariBlazorDemo.Shared.Database
{
	[Table("t_stock_song")]
	public class StockSong : IHistorySongProperty
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// StockSong ID
		// 有効な曲の ID は 1 から始まる（0 は未初期化）
		public Int32 StockSongId { get; set; }

		// ====================================================================
		// public プロパティー（IHistorySongProperty）
		// ====================================================================

		// AvailableSong ID
		public String AvailableSongId { get; set; } = String.Empty;

		// 登録者のユーザー ID
		public String UserId { get; set; } = String.Empty;

		// 登録日時 UTC（修正ユリウス日）
		public Double RequestTime { get; set; }

		// ====================================================================
		// public プロパティー（ISongProperty）
		// ====================================================================

		// 曲名
		public String SongName { get; set; } = String.Empty;

		// タイアップ名
		public String TieUpName { get; set; } = String.Empty;

		// 歌手名
		public String ArtistName { get; set; } = String.Empty;

		// 制作会社
		public String MakerName { get; set; } = String.Empty;

		// 動画制作者
		public String Worker { get; set; } = String.Empty;

		// 動画ファイル名
		public String Path { get; set; } = String.Empty;
	}
}
