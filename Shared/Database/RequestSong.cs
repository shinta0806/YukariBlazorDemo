﻿// ============================================================================
// 
// 予約された曲の情報
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Shared.Database
{
	[Table("t_request_song")]
	public class RequestSong : IHistorySongProperty
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// RequestSong ID
		// 有効な曲の ID は 1 から始まる（0 は未初期化）
		public Int32 RequestSongId { get; set; }

		// ソート順
		public Int32 Sort { get; set; }

		// 予約者
		[Required(ErrorMessage = "予約者を入力してください。")]
		public String UserName { get; set; } = String.Empty;

		// 予約コメント
		public String? Comment { get; set; }

		// 再生状態
		public PlayStatus PlayStatus { get; set; }

		// ====================================================================
		// public プロパティー（IHistorySongProperty）
		// ====================================================================

		// AvailableSong ID
		// 元の曲情報（サムネイル等）を参照する際に必要
		public String AvailableSongId { get; set; } = String.Empty;

		// 予約者のユーザー ID（ログインユーザーの場合のみ）
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

		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 必須項目が格納されているか
		// --------------------------------------------------------------------
		public Boolean IsValid()
		{
			return !String.IsNullOrEmpty(UserName) && !String.IsNullOrEmpty(AvailableSongId) && RequestTime > YbdConstants.INVALID_MJD && !String.IsNullOrEmpty(Path);
		}
	}
}
