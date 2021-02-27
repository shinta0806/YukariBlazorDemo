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
using System.IO;
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
			_serverSentEventsService = serverSentEventsService;
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

				// 予約者のユーザー ID が指定されている場合はその正当性を確認（なりすまし予約防止）
				using UserProfileContext userProfileContext = CreateUserProfileContext(out DbSet<RegisteredUser> registeredUsers, out DbSet<StockSong> stockSongs, out DbSet<HistorySong> historySongs);
				if (!String.IsNullOrEmpty(requestSong.UserId))
				{
					if (!IsTokenValid(registeredUsers, out RegisteredUser? loginUser) || requestSong.UserId != loginUser.Id)
					{
						return Unauthorized();
					}
				}

				// 追加する曲の位置は最後
				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);
				Int32 sort;
				if (requestSongs.Any())
				{
					sort = requestSongs.Max(x => x.Sort) + 1;
				}
				else
				{
					sort = 1;
				}
				requestSong.Sort = sort;

				// 予約追加
				requestSongs.Add(requestSong);
				requestSongContext.SaveChanges();

				if (!String.IsNullOrEmpty(requestSong.UserId))
				{
					// 予約者のユーザー ID が指定されている場合は履歴追加
					HistorySong historySong = new();
					YbdCommon.CopyHistorySongProperty(requestSong, historySong);
					historySongs.Add(historySong);

					// 後で歌う予定リストに追加されている場合はリストから削除
					StockSong? stockSong = SearchStockSongByRequestSong(stockSongs, requestSong);
					if (stockSong != null)
					{
						stockSongs.Remove(stockSong);
					}

					userProfileContext.SaveChanges();
				}

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

				// 削除対象より上にある予約を下へ
				IQueryable<RequestSong> downs = requestSongs.Where(x => x.Sort > deleteSong.Sort);
				foreach (RequestSong down in downs)
				{
					down.Sort--;
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
		// 予約をすべて削除
		// --------------------------------------------------------------------
		[HttpDelete, Route(YbdConstants.URL_REQUEST + YbdConstants.URL_ALL)]
		public IActionResult DeleteRequestAll()
		{
			try
			{
				// 管理者権限が必要
				using UserProfileContext userProfileContext = CreateUserProfileContext(out DbSet<RegisteredUser> registeredUsers, out _, out _);
				if (!IsTokenValid(registeredUsers, out RegisteredUser? loginUser) || !loginUser.IsAdmin)
				{
					return Unauthorized();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);
				if (!requestSongs.Any())
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
				if (IsEntityTagValid(YbdCommon.DateTimeToModifiedJulianDate(lastModified)))
				{
					Debug.WriteLine("GetRequestSongs() キャッシュ有効: " + query);
					return NotModified();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				// 追加ヘッダー
				AddTotalCountToHeader(requestSongs.Count());

				// 予約一覧
				Dictionary<String, String> parameters = YbdCommon.AnalyzeQuery(query);
				Int32 page = YbdCommon.GetPageFromQueryParameters(parameters);
				RequestSong[] results = requestSongs.OrderByDescending(x => x.Sort).Skip(YbdConstants.PAGE_SIZE * page).Take(YbdConstants.PAGE_SIZE).ToArray();
				EntityTagHeaderValue eTag = GenerateEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified));
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
				if (IsEntityTagValid(YbdCommon.DateTimeToModifiedJulianDate(lastModified)))
				{
					Debug.WriteLine("GetUserNames() キャッシュ有効: ");
					return NotModified();
				}

				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);
				String[] results = requestSongs.Where(x => x.UserId == String.Empty).Select(x => x.UserName).GroupBy(y => y).Select(z => z.Key).ToArray();

				// 追加ヘッダー
				AddTotalCountToHeader(results.Length);

				// 予約者名一覧
				EntityTagHeaderValue eTag = GenerateEntityTag(YbdCommon.DateTimeToModifiedJulianDate(lastModified));
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
					case YbdConstants.REQUEST_PARAM_VALUE_DOWN:
						result = MoveDownRequestSong(requestSongs, requestSong);
						break;
					case YbdConstants.REQUEST_PARAM_VALUE_NEXT:
						result = MoveNextRequestSong(requestSongs, requestSong);
						break;
					default:
						return BadRequest();
				}
				if (result is not OkResult)
				{
					return result;
				}

				requestSongContext.SaveChanges();
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

		// ====================================================================
		// DI
		// ====================================================================

		private readonly IServerSentEventsService _serverSentEventsService;

		// ====================================================================
		// private static メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// RequestSong の情報に合致する StockSong を検索
		// ＜例外＞ Exception
		// --------------------------------------------------------------------
		private static StockSong? SearchStockSongByRequestSong(DbSet<StockSong> stockSongs, RequestSong requestSong)
		{
			// ID で検索
			StockSong? result = stockSongs.SingleOrDefault(x => x.AvailableSongId == requestSong.AvailableSongId);
			if (result == null)
			{
				// フルパスで検索
				// 実際の運用時は外部アプリケーションが予約可能曲データベースを作成することが想定され、AvailableSongId が変更されている場合も想定されるため、パスでも検索する
				result = stockSongs.FirstOrDefault(x => x.Path == requestSong.Path);
			}
			if (result == null)
			{
				// ファイル名で検索
				result = stockSongs.FirstOrDefault(x => EF.Functions.Like(x.Path, $"%\\{Path.GetFileName(requestSong.Path)}%"));
			}

			return result;
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================

		// --------------------------------------------------------------------
		// 予約を下へ（ソート番号を小さく）
		// --------------------------------------------------------------------
		private IActionResult MoveDownRequestSong(DbSet<RequestSong> requestSongs, RequestSong requestSong)
		{
			// 交換対象の予約
			RequestSong? exchangeSong = requestSongs.Where(x => x.Sort < requestSong.Sort).OrderByDescending(x => x.Sort).FirstOrDefault();
			if (exchangeSong == null)
			{
				return NotAcceptable();
			}

			// 交換（順番入れ替え）
			(requestSong.Sort, exchangeSong.Sort) = (exchangeSong.Sort, requestSong.Sort);
			return Ok();
		}

		// --------------------------------------------------------------------
		// 予約を次の再生位置へ
		// --------------------------------------------------------------------
		private IActionResult MoveNextRequestSong(DbSet<RequestSong> requestSongs, RequestSong requestSong)
		{
			// 再生中の曲は次にできない
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

			return Ok();
		}

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
			_serverSentEventsService.SendEventAsync(data).NoWait();
		}
	}
}
