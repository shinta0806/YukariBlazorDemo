// ============================================================================
// 
// 予約された曲の情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;

namespace YukariBlazorDemo.Shared
{
	public class RequestSong : ISongProperty
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ID
		public Int32 Id { get; set; }

		// ソート方法
		public Int32 Sort { get; set; }

		// 予約者
		[Required(ErrorMessage = "予約者を入力してください。")]
		public String User { get; set; } = String.Empty;

		// 予約コメント
		public String? Comment { get; set; }

		// 再生状態
		public PlayStatus PlayStatus { get; set; }

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
		public String Maker { get; set; } = String.Empty;

		// 動画制作者
		public String Worker { get; set; } = String.Empty;

		// 動画ファイル名
		public String Path { get; set; } = String.Empty;

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 必須項目が格納されているか
		// --------------------------------------------------------------------
		public Boolean IsValid()
		{
			return !String.IsNullOrEmpty(Path) && !String.IsNullOrEmpty(User);
		}
	}
}
