﻿// ============================================================================
// 
// 予約 API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 予約は頻繁に更新されるため ShortCache 属性を付与
// ----------------------------------------------------------------------------

using Lib.AspNetCore.ServerSentEvents;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

using YukariBlazorDemo.Server.Attributes;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Database;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Server.Controllers
{
	[ShortCache]
	[Produces(ServerConstants.MIME_TYPE_JSON)]
	[Route(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS)]
	public class RequestSongController : ApiController
	{
		// ====================================================================
		// コンストラクター・デストラクター
		// ====================================================================

		// --------------------------------------------------------------------
		// コンストラクター
		// --------------------------------------------------------------------
		public RequestSongController(IServerSentEventsService serverSentEventsService)
		{
			mServerSentEventsService = serverSentEventsService;
		}

		// ====================================================================
		// API（ApiController）
		// ====================================================================

		// --------------------------------------------------------------------
		// 状態を返す
		// --------------------------------------------------------------------
		public override String ControllerStatus()
		{
			String status;
			try
			{
				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				// FirstOrDefault を使用すると列の不足を検出できる
				requestSongs.FirstOrDefault(x => x.RequestSongId == 0);

				status = "正常 / 予約曲数：" + requestSongs.Count();
			}
			catch (Exception excep)
			{
				status = "エラー / " + excep.Message;
				Debug.WriteLine("予約 API 状態取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			return status;
		}

		// ====================================================================
		// API（一般）
		// ====================================================================

		// --------------------------------------------------------------------
		// 予約を追加
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_REQUEST)]
		public IActionResult AddRequestSong([FromBody] RequestSong requestSong)
		{
			try
			{
				if (!requestSong.IsValid())
				{
					return BadRequest();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				Int32 sort;
				if (requestSongs.Count() == 0)
				{
					sort = 1;
				}
				else
				{
					sort = requestSongs.Max(x => x.Sort) + 1;
				}

				// 追加する曲の位置は最後
				requestSong.Sort = sort;
				requestSongs.Add(requestSong);
				requestSongContext.SaveChanges();

				SendSse(YbdConstants.SSE_DATA_REQUEST_CHANGED);
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約追加サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 予約を削除
		// --------------------------------------------------------------------
		[HttpDelete, Route(YbdConstants.URL_REQUEST + "{requestSongId?}")]
		public IActionResult DeleteRequest(String? requestSongId)
		{
			try
			{
				if (!Int32.TryParse(requestSongId, out Int32 requestSongIdNum))
				{
					return BadRequest();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				// 削除対象の予約
				RequestSong? deleteSong = requestSongs.SingleOrDefault(x => x.RequestSongId == requestSongIdNum);
				if (deleteSong == null)
				{
					return NotAcceptable();
				}
				requestSongs.Remove(deleteSong);
				requestSongContext.SaveChanges();

				SendSse(YbdConstants.SSE_DATA_REQUEST_CHANGED);
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約削除サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 予約を一括削除
		// --------------------------------------------------------------------
		[HttpDelete, Route(YbdConstants.URL_REQUEST + YbdConstants.URL_AT_ONCE + "{param?}")]
		public IActionResult DeleteRequestAtOnce(String? param)
		{
			try
			{
				if (param != YbdConstants.REQUEST_PARAM_VALUE_ALL)
				{
					return BadRequest();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);
				if (requestSongs.Count() == 0)
				{
					return NotAcceptable();
				}
				requestSongContext.Database.EnsureDeleted();
				requestSongContext.Database.EnsureCreated();

				SendSse(YbdConstants.SSE_DATA_REQUEST_CHANGED);
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約一括削除サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 予約一覧を返す
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_REQUEST + "{query?}")]
		public IActionResult GetRequestSongs(String? query)
		{
			try
			{
				// キャッシュチェック
				DateTime lastModified = ServerCommon.LastModified(ServerConstants.FILE_NAME_REQUEST_SONGS);
				if (IsValidEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified)))
				{
					Debug.WriteLine("GetRequestSongs() キャッシュ有効: " + query);
					return NotModified();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);
				Dictionary<String, String> parameters = YbdCommon.AnalyzeQuery(query);
				Int32 page = YbdCommon.GetPageFromQueryParameters(parameters);
				Int32 numResults = requestSongs.Count();
				RequestSong[] results = requestSongs.OrderByDescending(x => x.Sort).Skip(YbdConstants.PAGE_SIZE * page).Take(YbdConstants.PAGE_SIZE).ToArray();
				EntityTagHeaderValue eTag = GenerateEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified), YbdConstants.RESULT_PARAM_NAME_COUNT, numResults.ToString());
				return File(JsonSerializer.SerializeToUtf8Bytes(results), ServerConstants.MIME_TYPE_JSON, lastModified, eTag);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約一覧取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 予約者名一覧を返す（ゲストユーザーのみ）
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_GUEST_USER_NAMES)]
		public IActionResult GetUserNames()
		{
			try
			{
				// キャッシュチェック
				DateTime lastModified = ServerCommon.LastModified(ServerConstants.FILE_NAME_REQUEST_SONGS);
				if (IsValidEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified)))
				{
					Debug.WriteLine("GetUserNames() キャッシュ有効: ");
					return NotModified();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				String[] results = requestSongs.Where(x => x.UserId == String.Empty).Select(x => x.UserName).GroupBy(y => y).Select(z => z.Key).ToArray();
				Int32 numResults = results.Count();
				EntityTagHeaderValue eTag = GenerateEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified), YbdConstants.RESULT_PARAM_NAME_COUNT, numResults.ToString());
				return File(JsonSerializer.SerializeToUtf8Bytes(results), ServerConstants.MIME_TYPE_JSON, lastModified, eTag);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約者名一覧取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 指定曲に対する操作
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_REQUEST + "{requestSongId}")]
		public IActionResult ManageRequestSong(String requestSongId, [FromBody] String? command)
		{
			try
			{
				if (!Int32.TryParse(requestSongId, out Int32 requestSongIdNum))
				{
					return BadRequest();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				// 操作対象の予約
				RequestSong? requestSong = requestSongs.SingleOrDefault(x => x.RequestSongId == requestSongIdNum);
				if (requestSong == null)
				{
					return NotAcceptable();
				}

				IActionResult result;
				switch (command)
				{
					case YbdConstants.REQUEST_PARAM_VALUE_UP:
						result = MoveUpRequestSong(requestSongs, requestSong);
						break;
					default:
						return BadRequest();
				}
				if (result is not OkResult)
				{
					return result;
				}

				SendSse(YbdConstants.SSE_DATA_REQUEST_CHANGED);
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("指定曲操作サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

#if false
		// --------------------------------------------------------------------
		// 予約を下へ（ソート番号を小さく）
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_REQUEST + YbdConstants.URL_DOWN)]
		public IActionResult MoveDownRequestSong([FromBody] Int32 requestSongId)
		{
			try
			{
				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				// 移動対象の予約
				RequestSong? requestSong = requestSongs.SingleOrDefault(x => x.RequestSongId == requestSongId);
				if (requestSong == null)
				{
					return NotAcceptable();
				}

				// 交換対象の予約
				RequestSong? exchangeSong = requestSongs.Where(x => x.Sort < requestSong.Sort).OrderByDescending(x => x.Sort).FirstOrDefault();
				if (exchangeSong == null)
				{
					return NotAcceptable();
				}

				// 交換（順番入れ替え）
				(requestSong.Sort, exchangeSong.Sort) = (exchangeSong.Sort, requestSong.Sort);
				requestSongContext.SaveChanges();

				SendSse(YbdConstants.SSE_DATA_REQUEST_CHANGED);
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約を下へサーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 予約を次の再生位置へ
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_REQUEST + YbdConstants.URL_NEXT)]
		public IActionResult MoveNextRequestSong([FromBody] Int32 requestSongId)
		{
			try
			{
				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				// 移動対象の予約
				RequestSong? requestSong = requestSongs.SingleOrDefault(x => x.RequestSongId == requestSongId);
				if (requestSong == null)
				{
					return NotAcceptable();
				}
				if (requestSong.PlayStatus == PlayStatus.Playing || requestSong.PlayStatus == PlayStatus.Pause)
				{
					return Conflict();
				}

				Int32 next;
				RequestSong? playingSong = requestSongs.FirstOrDefault(x => x.PlayStatus == PlayStatus.Playing || x.PlayStatus == PlayStatus.Pause);
				if (playingSong != null && playingSong.Sort > requestSong.Sort)
				{
					// 移動対象の方が下にいるので、移動対象を上へ、他を下へ移動
					next = playingSong.Sort;
					IQueryable<RequestSong> downs = requestSongs.Where(x => requestSong.Sort < x.Sort && x.Sort <= next);
					foreach (RequestSong down in downs)
					{
						down.Sort--;
					}
				}
				else
				{
					// 移動対象の方が上にいるので、移動対象を下へ、他を上へ移動
					if (playingSong != null)
					{
						next = playingSong.Sort + 1;
					}
					else
					{
						next = 1;
					}
					IQueryable<RequestSong> ups = requestSongs.Where(x => next <= x.Sort && x.Sort < requestSong.Sort);
					foreach (RequestSong down in ups)
					{
						down.Sort++;
					}
				}

				// 対象を次の位置へ
				requestSong.Sort = next;
				if (requestSong.PlayStatus == PlayStatus.Played)
				{
					requestSong.PlayStatus = PlayStatus.Unplayed;
				}

				requestSongContext.SaveChanges();
				SendSse(YbdConstants.SSE_DATA_REQUEST_CHANGED);
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約を次の再生位置へサーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 予約を上へ（ソート番号を大きく）
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_REQUEST + YbdConstants.URL_UP)]
		public IActionResult MoveUpRequestSong([FromBody] Int32 requestSongId)
		{
			try
			{
				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				// 移動対象の予約
				RequestSong? requestSong = requestSongs.SingleOrDefault(x => x.RequestSongId == requestSongId);
				if (requestSong == null)
				{
					return NotAcceptable();
				}

				// 交換対象の予約
				RequestSong? exchangeSong = requestSongs.Where(x => x.Sort > requestSong.Sort).OrderBy(x => x.Sort).FirstOrDefault();
				if (exchangeSong == null)
				{
					return NotAcceptable();
				}

				// 交換（順番入れ替え）
				(requestSong.Sort, exchangeSong.Sort) = (exchangeSong.Sort, requestSong.Sort);
				requestSongContext.SaveChanges();

				SendSse(YbdConstants.SSE_DATA_REQUEST_CHANGED);
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約を上へサーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}
#endif

		// ====================================================================
		// DI
		// ====================================================================

		private readonly IServerSentEventsService mServerSentEventsService;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 予約を上へ（ソート番号を大きく）
		// --------------------------------------------------------------------
		private IActionResult MoveUpRequestSong(DbSet<RequestSong> requestSongs, RequestSong requestSong)
		{
			// 交換対象の予約
			RequestSong? exchangeSong = requestSongs.Where(x => x.Sort > requestSong.Sort).OrderBy(x => x.Sort).FirstOrDefault();
			if (exchangeSong == null)
			{
				return NotAcceptable();
			}

			// 交換（順番入れ替え）
			(requestSong.Sort, exchangeSong.Sort) = (exchangeSong.Sort, requestSong.Sort);
			return Ok();
		}

		// --------------------------------------------------------------------
		// Server-Sent Events で通知
		// --------------------------------------------------------------------
		private void SendSse(String data)
		{
			mServerSentEventsService.SendEventAsync(data).NoWait();
		}
	}
}
