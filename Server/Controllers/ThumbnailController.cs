// ============================================================================
// 
// サムネイル API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
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
		// AvailableSong.Id に対応するサムネイルを返す
		// --------------------------------------------------------------------
		[HttpGet, Route("{id}")]
		public IActionResult GetThumbnail(String? id)
		{
			try
			{
				using AvailableSongContext availableSongContext = new();
				if (availableSongContext.AvailableSongs == null)
				{
					throw new Exception();
				}
				AvailableSong availableSong = availableSongContext.AvailableSongs.First(x => x.Id == id);

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
					Thread.Sleep(random.Next(500, 1000));
				}

				Thumbnail thumbnail = thumbnailContext.Thumbnails.First(x => x.Path == availableSong.Path);
				DateTimeOffset lastModified = new DateTimeOffset(ServerCommon.ModifiedJulianDateToDateTime(thumbnail.LastModified));
				EntityTagHeaderValue eTag = ServerCommon.GenerateEntityTag(thumbnail.LastModified);
				return File(thumbnail.Bitmap, thumbnail.Mime, lastModified, eTag);
			}
			catch (Exception excep)
			{
				if (DefaultThumbnail != null)
				{
					// サムネイルが無いのでデフォルトサムネイル（NoImage）を返す
					return File(DefaultThumbnail.Bitmap, DefaultThumbnail.Mime, ServerConstants.INVALID_DATE_OFFSET, ServerConstants.INVALID_ETAG);
				}

				Debug.WriteLine("サムネイル取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
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
					throw new Exception("デフォルトサムネイルが作成できませんでした。" + ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + " フォルダーがあるか確認してください。");
				}
				using ThumbnailContext thumbnailContext = new();
				if (thumbnailContext.Thumbnails == null)
				{
					throw new Exception();
				}

				// Where を使用すると列の不足を検出できる
				thumbnailContext.Thumbnails.Where(x => x.Id == 0).FirstOrDefault();

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
