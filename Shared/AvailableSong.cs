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
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public AvailableSong()
		{
		}

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public AvailableSong(String id, NameAndRuby song, NameAndRuby tieUp, NameAndRuby artist, NameAndRuby maker, String worker, String path, Double lastModified, Int64 fileSize)
		{
			// ID
			Id = id;

			// 曲名
			SongName = song.Name;
			SongRuby = song.Ruby;

			// タイアップ名
			TieUpName = tieUp.Name;
			TieUpRuby = tieUp.Ruby;

			// 歌手名
			ArtistName = artist.Name;
			ArtistRuby = artist.Ruby;

			// 制作会社名
			MakerName = maker.Name;
			MakerRuby = maker.Ruby;

			// その他
			Worker = worker;
			Path = path;
			LastModified = lastModified;
			FileSize = fileSize;
		}

		// ====================================================================
		// public プロパティー
		// ====================================================================

		// ID
		// Column 属性を指定する必要は無いが、指定可能であることの備忘録として指定
		[Key]
		[Column("AvailableSongId")]
		public String Id { get; set; } = String.Empty;

		// 更新日時 UTC（修正ユリウス日）
		public Double LastModified { get; set; }

		// 動画ファイルサイズ
		public Int64 FileSize { get; set; }

		// 曲名フリガナ
		public String SongRuby { get; set; } = String.Empty;

		// タイアップ名フリガナ
		public String TieUpRuby { get; set; } = String.Empty;

		// 歌手名フリガナ
		public String ArtistRuby { get; set; } = String.Empty;

		// 制作会社名フリガナ
		public String MakerRuby { get; set; } = String.Empty;

		// ====================================================================
		// public プロパティー（ISongProperty）
		// ====================================================================

		// 曲名
		public String SongName { get; set; } = String.Empty;

		// タイアップ名
		public String TieUpName { get; set; } = String.Empty;

		// 歌手名
		public String ArtistName { get; set; } = String.Empty;

		// 制作会社名
		public String MakerName { get; set; } = String.Empty;

		// 動画制作者
		public String Worker { get; set; } = String.Empty;

		// 動画ファイル名
		[Required]
		public String Path { get; set; } = String.Empty;

	}
}
