// ============================================================================
// 
// API 基底クラス
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using System;
using System.Net;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Server.Controllers
{
	public abstract class ApiController : Controller
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public ApiController()
		{
			INVALID_ETAG = GenerateEntityTag(ServerConstants.INVALID_MJD);
		}

		// ====================================================================
		// API
		// ====================================================================

		// --------------------------------------------------------------------
		// 状態を返す
		// --------------------------------------------------------------------
		[Produces(ServerConstants.MIME_TYPE_JSON)]
		[HttpGet, Route(YbdConstants.URL_STATUS)]
		public abstract String ControllerStatus();

		// ====================================================================
		// protected メンバー定数
		// ====================================================================

		// 日付が指定されていない場合の ETAG
		protected readonly EntityTagHeaderValue INVALID_ETAG;

		// ====================================================================
		// protected メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// ETag 生成
		// --------------------------------------------------------------------
		protected EntityTagHeaderValue GenerateEntityTag(Double lastModified)
		{
			return new EntityTagHeaderValue("\"" + lastModified.ToString() + "\"");
		}

		// --------------------------------------------------------------------
		// ETag 生成（パラメーター 1 つ）
		// --------------------------------------------------------------------
		protected EntityTagHeaderValue GenerateEntityTag(Double lastModified, String paramName, String paramValue)
		{
			return new EntityTagHeaderValue("\"" + lastModified.ToString() + "&" + paramName + "=" + paramValue + "\"");
		}

		// --------------------------------------------------------------------
		// クライアント側から送られてきた ETag が有効か
		// --------------------------------------------------------------------
		protected Boolean IsValidEntityTag(Double lastModified)
		{
			HttpContext.Request.Headers.TryGetValue("If-None-Match", out StringValues values);
			for (Int32 i = 0; i < values.Count; i++)
			{
				String str = values[i].Trim('"');
				Int32 pos = str.IndexOf('&');
				if (pos >= 0)
				{
					str = str.Substring(0, pos);
				}
				if (str == lastModified.ToString())
				{
					return true;
				}
			}
			return false;
		}

		// --------------------------------------------------------------------
		// クライアント側のキャッシュが有効
		// --------------------------------------------------------------------
		protected StatusCodeResult NotModified()
		{
			return StatusCode((Int32)HttpStatusCode.NotModified);
		}

		// ====================================================================
		// private メンバー定数
		// ====================================================================


	}
}
