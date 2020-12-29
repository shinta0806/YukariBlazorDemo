// ============================================================================
// 
// 予約可能な曲（ストレージに保存されている曲）の情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukariBlazorDemo.Shared
{
	[Table("t_available_song")]
	public class AvailableSong : ISongProperty
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ID
		// Column 属性を指定する必要は無いが、指定可能であることの備忘録として指定
		[Key]
		[Column("AvailableSongId")]
		public Int32 Id { get; set; }

		// 更新日時 UTC（修正ユリウス日）
		public Double LastModified { get; set; }

		// 動画ファイルサイズ
		public Int64 FileSize { get; set; }

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
		[Required]
		public String Path { get; set; } = String.Empty;

	}
}
