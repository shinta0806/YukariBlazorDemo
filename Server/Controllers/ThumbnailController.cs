using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using YukariBlazorDemo.Server.Database;
using System.Threading;
using System.Diagnostics;
using YukariBlazorDemo.Shared;
using YukariBlazorDemo.Server.Misc;

namespace YukariBlazorDemo.Server.Controllers
{
	[Route("api/thumbnail")]
	public class ThumbnailController : Controller
	{
		public static Thumbnail? DefaultThumbnail { get; set; }

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
			catch (Exception)
			{
				if (DefaultThumbnail != null)
				{
					return File(DefaultThumbnail.Bitmap, DefaultThumbnail.Mime);
				}
				return BadRequest();
			}
		}

	}
}
