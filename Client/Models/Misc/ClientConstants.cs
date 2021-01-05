// ============================================================================
// 
// クライアント側共通で使用する定数
// 
// ============================================================================

// ----------------------------------------------------------------------------
//
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YukariBlazorDemo.Client.Models.Misc
{
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

		// 初期状態を表す文字列
		public const String INIT_STRING = "__Init__";
	}
}
