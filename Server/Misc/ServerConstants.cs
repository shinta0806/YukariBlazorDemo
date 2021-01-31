// ============================================================================
// 
// サーバー側共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;

using YukariBlazorDemo.Shared.Misc;

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

		// ユーザーデータベース
		public const String FILE_NAME_USER_PROFILES = "UserProfiles" + FILE_EXT_SQLITE3;

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
		// 認証
		// --------------------------------------------------------------------

		// トークン生成用の秘密鍵（16 文字以上）
		//public const String TOKEN_SECRET_KEY = "1234567890123456";

		// トークン生成要の秘密鍵の最低長さ
		public const Int32 TOKEN_SECRET_KEY_LENGTH_MIN = 16;

		// 発行者名
		public const String TOKEN_ISSUER = "YukariBlazorDemo";
	}
}
