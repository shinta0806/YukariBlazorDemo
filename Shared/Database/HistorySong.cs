// ============================================================================
// 
// 過去日も含めた予約履歴の情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukariBlazorDemo.Shared.Database
{
	[Table("t_history_song")]
	public class HistorySong : ISongProperty
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// HistorySong ID
		// 有効な曲の ID は 1 から始まる（0 は未初期化）
		public Int32 HistorySongId { get; set; }

		// AvailableSong ID
		public String AvailableSongId { get; set; } = String.Empty;

		// 予約者のユーザー ID
		public String UserId { get; set; } = String.Empty;

		// 予約日時 UTC（修正ユリウス日）
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
