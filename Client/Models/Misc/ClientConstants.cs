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

		// API 状態取得
		public const String API_STATUS_ERROR_CANNOT_GET = "状態を取得できません。";
		public const String API_STATUS_ERROR_CANNOT_CONNECT = "サーバーと通信できません。";

	}
}
