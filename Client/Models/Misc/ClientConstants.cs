// ============================================================================
// 
// クライアント側共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;

using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Client.Models.Misc
{
	// ====================================================================
	// public デリゲート
	// ====================================================================

	// 一般通知（引数無し）
	public delegate void NotifyDelegate();

	public class ClientConstants
	{
		// ====================================================================
		// public 定数
		// ====================================================================

		// インライン Loading の div タグ（"|" は砂時計を上下中央に配置するため）
		public const String DIV_MINI_LOADING = "<div class='miniloading'>|</div>";

		// エラーページ
		public const String ERROR_PARAM_NAME_MESSAGE = "message";
		public const String ERROR_PARAM_NAME_TRACE = "trace";
		public const String ERROR_PARAM_NAME_TYPE = "type";

		// エラーメッセージ
		public const String ERROR_MESSAGE_BAD_REQUEST = "サーバーに対する指定方法が不正です。";
		public const String ERROR_MESSAGE_CANNOT_CONNECT = "サーバーに接続できませんでした。";
		public const String ERROR_MESSAGE_INTERNAL_SERVER_ERROR = "サーバー内部でエラーが発生しました。";
		public const String ERROR_MESSAGE_UNAUTHORIZED = "ログインしていないか、または、権限がありません。";
		public const String ERROR_MESSAGE_UNEXPECTED = "予期しないエラーが発生しました。";

		// 初期状態を表す文字列
		public const String INIT_STRING = "__Init__";

		// 日付の標準書式
		public const String DATE_TIME_FORMAT = "yyyy/MM/dd HH:mm:ss";

		// ログインページのパラメーター
		public const String LOGIN_PARAM_REDIRECT = "redirect";

		// 権限
		public const String ROLE_NAME_ADMIN = YbdConstants.ADMIN_NAME;
		public const String ROLE_NAME_GENERAL = "general";
	}
}
