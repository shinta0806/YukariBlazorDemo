// ============================================================================
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
using System.Threading.Tasks;
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
		[HttpDelete, Route(YbdConstants.URL_REQUEST + "{id?}")]
		public IActionResult DeleteRequest()
		{
			try
			{
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
				Debug.WriteLine("予約全削除サーバーエラー：\n" + excep.Message);
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
		// 予約を下へ（ソート番号を小さく）
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_REQUEST + YbdConstants.URL_DOWN + "{requestSongId?}")]
		public IActionResult MoveDownRequestSong(String? requestSongId, [FromBody] Int32 dummy)
		{
			try
			{
				if (!Int32.TryParse(requestSongId, out Int32 requestSongIdNum))
				{
					return BadRequest();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				// 移動対象の予約
				RequestSong? requestSong = requestSongs.FirstOrDefault(x => x.RequestSongId == requestSongIdNum);
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
		// 予約を上へ（ソート番号を大きく）
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_REQUEST + YbdConstants.URL_UP + "{requestSongId?}")]
		public IActionResult MoveUpRequestSong(String? requestSongId, [FromBody] Int32 dummy)
		{
			try
			{
				if (!Int32.TryParse(requestSongId, out Int32 requestSongIdNum))
				{
					return BadRequest();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				// 移動対象の予約
				RequestSong? requestSong = requestSongs.FirstOrDefault(x => x.RequestSongId == requestSongIdNum);
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

		// ====================================================================
		// DI
		// ====================================================================

		private readonly IServerSentEventsService mServerSentEventsService;

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// Server-Sent Events で通知
		// --------------------------------------------------------------------
		private void SendSse(String data)
		{
			mServerSentEventsService.SendEventAsync(data).NoWait();
		}
	}
}
