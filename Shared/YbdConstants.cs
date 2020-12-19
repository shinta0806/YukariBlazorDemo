// ============================================================================
// 
// YukariBlazorDemo 共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YukariBlazorDemo.Shared
{
	// ====================================================================
	// public 列挙子
	// ====================================================================

	// --------------------------------------------------------------------
	// 再生状態
	// --------------------------------------------------------------------
	public enum PlayStatus
	{
		Unplayed,
		Playing,
		Pause,
		Played,
	}

	// --------------------------------------------------------------------
	// 検索詳細条件
	// --------------------------------------------------------------------
	public enum SearchDetailCondition
	{
		SongName,
		TieUpName,
		ArtistName,
		Maker,
		Worker,
		Path,
		__End__,
	}

	// --------------------------------------------------------------------
	// 検索方法
	// --------------------------------------------------------------------
	public enum SearchWordType
	{
		AnyWord,
		Detail,
	}

	public class YbdConstants
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		// SearchDetailCondition に対応する名称
		public static readonly String[] SEARCH_DETAIL_CONDITION_NAMES = { "曲名", "タイアップ名", "歌手名", "制作会社", "カラオケ動画制作者", "ファイル名" };

		// SearchDetailCondition に対応する URL パラメーター
		public static readonly String[] SEARCH_DETAIL_PARAM_NAMES = { "songname", "tieupname", "artistname", "maker", "worker", "filename" };

		// AnyWord に対応する URL パラメーター
		public const String PARAM_NAME_ANY_WORD = "anyword";

	}
}
