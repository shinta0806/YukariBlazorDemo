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
	// 検索結果並び順
	// --------------------------------------------------------------------
	public enum SearchResultSort
	{
		Latest,
		SongName,
		ArtistName,
		FileSize,
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

		// --------------------------------------------------------------------
		// API URL
		// --------------------------------------------------------------------

		public const String URL_API = "api/";
		public const String URL_DELETE_ALL = "deleteall/";
		public const String URL_ID = "id/";
		public const String URL_NEXT = "next/";
		public const String URL_PLAY_OR_PAUSE = "playorpause/";
		public const String URL_PLAYER = "player/";
		public const String URL_PLAYING = "playing/";
		public const String URL_PREV = "prev/";
		public const String URL_REQUEST = "request/";
		public const String URL_REQUEST_SONGS = "requestsongs/";
		public const String URL_SEARCH = "search/";
		public const String URL_THUMBNAIL = "thumbnail/";
		public const String URL_WORD = "word/";

		// --------------------------------------------------------------------
		// 検索関連
		// --------------------------------------------------------------------

		// SearchDetailCondition に対応する名称
		public static readonly String[] SEARCH_DETAIL_CONDITION_NAMES = { "曲名", "タイアップ名", "歌手名", "制作会社", "カラオケ動画制作者", "ファイル名" };

		// SearchDetailCondition に対応する URL パラメーター
		public static readonly String[] SEARCH_DETAIL_PARAM_NAMES = { "songname", "tieupname", "artistname", "maker", "worker", "filename" };

		// 検索方法に AnyWord を指定する URL パラメーター
		public const String SEARCH_PARAM_NAME_ANY_WORD = "anyword";

		// SearchResultSort に対応する名称
		public static readonly String[] SEARCH_RESULT_SORT_NAMES = { "新着順", "曲名順", "歌手名順", "サイズ順" };

		// SearchResultSort を指定する URL パラメーター
		public const String SEARCH_PARAM_NAME_SORT = "sort";
	}
}
