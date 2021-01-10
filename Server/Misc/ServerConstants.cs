// ============================================================================
// 
// サーバー側共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;

namespace YukariBlazorDemo.Server.Misc
{
	public class ServerConstants
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		// --------------------------------------------------------------------
		// フォルダー名
		// --------------------------------------------------------------------

		// サムネイル画像
		public const String FOLDER_NAME_SAMPLE_DATA_IMAGES = "SampleDataImages\\";

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------

		// 予約可能曲データベース
		public const String FILE_NAME_AVAILABLE_SONGS = "AvailableSongs" + FILE_EXT_SQLITE3;

		// 予約された曲データベース
		public const String FILE_NAME_REQUEST_SONGS = "RequestSongs" + FILE_EXT_SQLITE3;

		// 登録ユーザーデータベース
		public const String FILE_NAME_REGISTERED_USERS = "RegisteredUsers" + FILE_EXT_SQLITE3;

		// --------------------------------------------------------------------
		// ファイル拡張子
		// --------------------------------------------------------------------

		// SQLite3
		public const String FILE_EXT_SQLITE3 = ".sqlite3";

		// --------------------------------------------------------------------
		// MimeType
		// --------------------------------------------------------------------

		// GIF
		public const String MIME_TYPE_GIF = "image/gif";

		// JPEG
		public const String MIME_TYPE_JPEG = "image/jpeg";

		// JSON
		public const String MIME_TYPE_JSON = "application/json";

		// PNG
		public const String MIME_TYPE_PNG = "image/png";

		// --------------------------------------------------------------------
		// 日付が指定されていない場合
		// --------------------------------------------------------------------

		// 日付が指定されていない場合にこの日付を使う
		public static readonly DateTime INVALID_DATE = new DateTime(1900, 1, 1);

		// 日付が指定されていない場合の DateTimeOffset
		public static readonly DateTimeOffset INVALID_DATE_OFFSET = new DateTimeOffset(INVALID_DATE);

		// 日付が指定されていない場合の修正ユリウス日
		public static readonly Double INVALID_MJD = ServerCommon.DateTimeToModifiedJulianDate(INVALID_DATE);

		// --------------------------------------------------------------------
		// 認証
		// --------------------------------------------------------------------

		// トークン生成用の秘密鍵（16 文字以上）
		public const String TOKEN_SECRET_KEY = "1234567890123456";

		// 発行者名
		public const String TOKEN_ISSUER = "YukariBlazorDemo";
	}
}
