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
		// API 等 URL
		// --------------------------------------------------------------------

		public const String URL_ALL = "all/";
		public const String URL_API = "api/";
		public const String URL_AUTH = "auth/";
		public const String URL_CURRENT_USER = "currentuser/";
		public const String URL_EXTEND = "extend/";
		public const String URL_GUEST_USER_NAMES = "guestusernames/";
		public const String URL_ID = "id/";
		public const String URL_INFO = "info/";
		public const String URL_IS_ADMIN_REGISTERED = "isadminregistered/";
		public const String URL_IS_LOGGED_IN = "isloggedin/";
		public const String URL_LOGIN = "login/";
		public const String URL_LOGOUT = "logout/";
		public const String URL_MOVIE = "movie/";
		public const String URL_NAME = "name/";
		public const String URL_NEXT = "next/";
		public const String URL_PASSWORD = "password/";
		public const String URL_PLAY_OR_PAUSE = "playorpause/";
		public const String URL_PLAYER = "player/";
		public const String URL_PLAYING = "playing/";
		public const String URL_PREV = "prev/";
		public const String URL_PUBLIC = "public/";
		public const String URL_REQUEST = "request/";
		public const String URL_REQUEST_SONGS = "requestsongs/";
		public const String URL_SEARCH = "search/";
		public const String URL_SSE = "sse/";
		public const String URL_STATUS = "status/";
		public const String URL_THUMBNAIL = "thumbnail/";
		public const String URL_USERS = "users/";
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
		// 予約関連
		// --------------------------------------------------------------------

		// 予約を上へ
		public const String REQUEST_PARAM_VALUE_UP = "up";

		// 予約を下へ
		public const String REQUEST_PARAM_VALUE_DOWN = "down";

		// 予約を次に再生
		public const String REQUEST_PARAM_VALUE_NEXT = "next";

		// 未再生にする
		public const String REQUEST_PARAM_VALUE_UNPLAYED = "unplayed";

		// 再生済にする
		public const String REQUEST_PARAM_VALUE_PLAYED = "played";

		// --------------------------------------------------------------------
		// 認証関連
		// --------------------------------------------------------------------

		// 管理者の名前
		public const String ADMIN_NAME = "admin";

		// ID とトークンの区切り（Base64URL とピリオド以外の文字）
		public const String TOKEN_DELIM = ",";

		// サムネイル最大幅 [pixel]
		public const Int32 USER_THUMBNAIL_WIDTH_MAX = 300;

		// サムネイル最大高さ [pixel]
		public const Int32 USER_THUMBNAIL_HEIGHT_MAX = 300;

		// サムネイル最大サイズ [byte]
		public const Int32 USER_THUMBNAIL_LENGTH_MAX = 150 * 1024;

		// --------------------------------------------------------------------
		// Server-Sent Events 関連
		// --------------------------------------------------------------------

		// 予約更新
		public const String SSE_DATA_REQUEST_CHANGED = "RequestChanged";
	}
}
