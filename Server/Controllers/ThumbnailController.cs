﻿// ============================================================================
// 
// 動画サムネイル API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// サムネイルは原則として更新されないため OneYearCache 属性を付与
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using YukariBlazorDemo.Server.Attributes;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Database;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Server.Controllers
{
	[OneYearCache]
	[Route(YbdConstants.URL_API + YbdConstants.URL_MOVIE)]
	public class ThumbnailController : ApiController
	{
		// ====================================================================
		// public static プロパティー
		// ====================================================================

		// サムネイルが見つからない場合に返すデフォルトのサムネイル
		public static Thumbnail? DefaultThumbnail { get; set; }

		// ====================================================================
		// API（ApiController）
		// ====================================================================

		// --------------------------------------------------------------------
		// 状態を返す
		// --------------------------------------------------------------------
		[ShortCache]
		public override String ControllerStatus()
		{
			String status;
			try
			{
				if (DefaultThumbnail == null)
				{
					throw new Exception("デフォルトサムネイルが作成できませんでした。" + ServerConstants.FOLDER_NAME_SAMPLE_DATA_IMAGES + " フォルダーがあるか確認してください。");
				}
				using ThumbnailContext thumbnailContext = CreateThumbnailContext(out DbSet<Thumbnail> thumbnails);

				// FirstOrDefault を使用すると列の不足を検出できる
				thumbnails.FirstOrDefault(x => x.Id == 0);

				status = "正常 / サムネイル数：" + thumbnails.Count();
			}
			catch (Exception excep)
			{
				status = "エラー / " + excep.Message;
				Debug.WriteLine("サムネイル API 状態取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			return status;
		}

		// ====================================================================
		// API（一般）
		// ====================================================================

		// --------------------------------------------------------------------
		// AvailableSong.Id に対応するサムネイルを返す
		// サムネイルはめったに更新されないため OneYearCache 属性を付与する
		// --------------------------------------------------------------------
		[OneYearCache]
		[HttpGet, Route(YbdConstants.URL_THUMBNAIL + "{id}")]
		public IActionResult GetThumbnail(String? id)
		{
			try
			{
				using AvailableSongContext availableSongContext = CreateAvailableSongContext(out DbSet<AvailableSong> availableSongs);
				AvailableSong? availableSong = availableSongs.SingleOrDefault(x => x.Id == id);
				if (availableSong == null)
				{
					// ID が見つからない
					return NotAcceptable();
				}

				using ThumbnailContext thumbnailContext = CreateThumbnailContext(out DbSet<Thumbnail> thumbnails);
				Thumbnail? thumbnail = thumbnails.FirstOrDefault(x => x.Path == availableSong.Path);
				if (thumbnail == null)
				{
					// ID に対応するサムネイルが無い
					thumbnail = DefaultThumbnail;
					if (thumbnail == null)
					{
						throw new Exception();
					}
				}

				// キャッシュチェック
				if (IsEntityTagValid(thumbnail.LastModified))
				{
					Debug.WriteLine("GetThumbnail() キャッシュ有効: " + id);
					return NotModified();
				}

				// 実際の運用時はサムネイルの返却に時間がかかることを想定
				Random random = new();
				Thread.Sleep(random.Next(500, 1000));

				Debug.WriteLine("GetThumbnail() キャッシュ無し: " + id);
				DateTimeOffset lastModified = new DateTimeOffset(YbdCommon.ModifiedJulianDateToDateTime(thumbnail.LastModified));
				EntityTagHeaderValue eTag = GenerateEntityTag(thumbnail.LastModified);
				return File(thumbnail.Bitmap, thumbnail.Mime, lastModified, eTag);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("サムネイル取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}
	}
}
