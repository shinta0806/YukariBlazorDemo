﻿// ============================================================================
// 
// 登録ユーザーの曲履歴プロパティーを規定するインターフェース
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using System;

namespace YukariBlazorDemo.Shared.Database
{
	public interface IHistorySongProperty : ISongProperty
	{
		// ====================================================================
		// public プロパティー
		// ====================================================================

		// AvailableSong ID
		public String AvailableSongId { get; set; }

		// 予約者のユーザー ID
		public String UserId { get; set; }

		// 予約日時 UTC（修正ユリウス日）
		public Double RequestTime { get; set; }
	}
}
