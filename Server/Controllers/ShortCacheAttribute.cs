// ============================================================================
// 
// キャッシュの有効期限を短くする属性
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc.Filters;

using System;

namespace YukariBlazorDemo.Server.Controllers
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class ShortCacheAttribute : ActionFilterAttribute
	{
		// ====================================================================
		// public メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// action result 実行前
		// OnResultExecuted() では既に応答が始まっているのでキャッシュコントロールを変更できない
		// --------------------------------------------------------------------
		public override void OnResultExecuting(ResultExecutingContext context)
		{
			// ブラウザに内容をキャッシュさせるが、1 秒後には有効期限が切れて変更確認が来る
			// max-age=0 でも大丈夫そうだが、キャッシュは保持して欲しいので念のため 1 にしておく
			context.HttpContext.Response.Headers["Cache-Control"] = "max-age=1";
			base.OnResultExecuting(context);
		}

	}
}
