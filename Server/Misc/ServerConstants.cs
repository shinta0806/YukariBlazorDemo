// ============================================================================
// 
// サーバー側共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using Microsoft.Net.Http.Headers;

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
		public const String FOLDER_NAME_SAMPLE_DATA_IMAGES = "SampleDataImages\\";

		// --------------------------------------------------------------------
		// ファイル名
		// --------------------------------------------------------------------

		// 予約可能曲データベース
		public const String FILE_NAME_AVAILABLE_SONGS = "AvailableSongs.sqlite3";

		// 予約された曲データベース
		public const String FILE_NAME_REQUEST_SONGS = "RequestSongs.sqlite3";

		// --------------------------------------------------------------------
		// MimeType
		// --------------------------------------------------------------------

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

	}
}
