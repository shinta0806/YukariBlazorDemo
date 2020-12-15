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

		[HttpGet, Route("{id}")]
		public IActionResult GetThumbnail(String? id)
		{
			try
			{
				AvailableSong? availableSong = ServerCommon.AvailableSongById(id);
				if (availableSong == null)
				{
					throw new Exception();
				}
				using ThumbnailContext thumbnailContext = new();
				if (thumbnailContext.Thumbnails == null)
				{
					throw new Exception();
				}
				Thumbnail thumbnail = thumbnailContext.Thumbnails.First(x => x.Id == availableSong.Id);
				return File(thumbnail.Bitmap, thumbnail.Mime);
			}
			catch (Exception)
			{
				return BadRequest();
			}
		}

	}
}
