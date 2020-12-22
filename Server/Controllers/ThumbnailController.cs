// ============================================================================
// 
// サムネイル API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Controllers
{
	[Route(YbdConstants.URL_API + YbdConstants.URL_THUMBNAIL)]
	public class ThumbnailController : Controller
	{
		// ====================================================================
		// public static プロパティー
		// ====================================================================

		// サムネイルが見つからない場合に返すデフォルトのサムネイル
		public static Thumbnail? DefaultThumbnail { get; set; }

		// ====================================================================
		// API
		// ====================================================================

		// --------------------------------------------------------------------
		// ファイル名に対応するサムネイルを返す
		// --------------------------------------------------------------------
		[HttpGet, Route("{path}")]
		public IActionResult GetThumbnail(String? path)
		{
			try
			{
				using ThumbnailContext thumbnailContext = new();
				if (thumbnailContext.Thumbnails == null)
				{
					throw new Exception();
				}

				// 実際の運用時はサムネイルの返却に時間がかかることを想定
				Random random = new();
				if (random.Next(5) == 0)
				{
					// 数回に 1 回はキャッシュがヒットする想定で時間がかからない
				}
				else
				{
					// すこし時間をかける
					Thread.Sleep(random.Next(100, 300));
				}

				Thumbnail thumbnail = thumbnailContext.Thumbnails.First(x => x.Path == path);
				return File(thumbnail.Bitmap, thumbnail.Mime);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("サムネイル取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);

				if (DefaultThumbnail != null)
				{
					return File(DefaultThumbnail.Bitmap, DefaultThumbnail.Mime);
				}
				return BadRequest();
			}
		}

		// --------------------------------------------------------------------
		// 状態を返す
		// --------------------------------------------------------------------
		[Produces(ServerConstants.MIME_TYPE_JSON)]
		[HttpGet, Route(YbdConstants.URL_STATUS)]
		public String ThumbnailControllerStatus()
		{
			String status;
			try
			{
				if (DefaultThumbnail == null)
				{
					throw new Exception("デフォルトサムネイルが作成できませんでした。SampleDataImages フォルダーがあるか確認してください。");
				}
				using ThumbnailContext thumbnailContext = new();
				if (thumbnailContext.Thumbnails == null)
				{
					throw new Exception();
				}
				status = "正常 / サムネイル数：" + thumbnailContext.Thumbnails.Count();
			}
			catch (Exception excep)
			{
				status = "エラー / " + excep.Message;
				Debug.WriteLine("サムネイル API 状態取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			return status;
		}


	}
}
