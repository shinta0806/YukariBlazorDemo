// ============================================================================
// 
// YukariBlazorDemo 共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;

namespace YukariBlazorDemo.Shared.Misc
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
		MakerName,
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

		public const String URL_ADD = "add/";
		public const String URL_API = "api/";
		public const String URL_AUTH = "auth/";
		public const String URL_DELETE_ALL = "deleteall/";
		public const String URL_GUEST_USER_NAMES = "guestusernames/";
		public const String URL_ID = "id/";
		public const String URL_IS_ADMIN_REGISTERED = "isadminregistered/";
		public const String URL_IS_LOGGED_IN = "isloggedin/";
		public const String URL_LIST = "list/";
		public const String URL_LOGIN = "login/";
		public const String URL_LOGIN_INFO = "logininfo/";
		public const String URL_LOGOUT = "logout/";
		public const String URL_MOVIE_THUMBNAIL = "moviethumbnail/";
		public const String URL_NEXT = "next/";
		public const String URL_PLAY_OR_PAUSE = "playorpause/";
		public const String URL_PLAYER = "player/";
		public const String URL_PLAYING = "playing/";
		public const String URL_PREV = "prev/";
		public const String URL_PUBLIC_USER_INFO = "publicuserinfo/";
		public const String URL_PUBLIC_USER_THUMBNAIL = "publicuserthumbnail/";
		public const String URL_REQUEST = "request/";
		public const String URL_REQUEST_SONGS = "requestsongs/";
		public const String URL_SEARCH = "search/";
		public const String URL_SET_USER_THUMBNAIL = "setuserthumbnail/";
		public const String URL_STATUS = "status/";
		public const String URL_WORD = "word/";

		// --------------------------------------------------------------------
		// 検索関連
		// --------------------------------------------------------------------

		// AnyWord に対応する名称
		public const String SEARCH_ANY_WORD_CONDITION_NAME = "すべて";

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

		// 表示ページを指定する URL パラメーター
		public const String SEARCH_PARAM_NAME_PAGE = "page";

		// 1 ページ当たりの結果の数
		public const Int32 PAGE_SIZE = 20;

		// 結果の数パラメーター
		public const String RESULT_PARAM_NAME_COUNT = "count";

		// --------------------------------------------------------------------
		// 認証関連
		// --------------------------------------------------------------------

		// 管理者の名前
		public const String ADMIN_NAME = "admin";

		// ID とトークンの区切り（Base64URL とピリオド以外の文字）
		public const String TOKEN_DELIM = ",";

	}
}
