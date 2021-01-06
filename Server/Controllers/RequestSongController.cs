// ============================================================================
// 
// 予約 API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 予約は頻繁に更新されるため ShortCache 属性を付与
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

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
				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception("データベースにアクセスできません。");
				}

				// FirstOrDefault を使用すると列の不足を検出できる
				requestSongContext.RequestSongs.FirstOrDefault(x => x.RequestSongId == 0);

				status = "正常 / 予約曲数：" + requestSongContext.RequestSongs.Count();
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
					throw new Exception();
				}

				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}

				Int32 sort;
				if (requestSongContext.RequestSongs.Count() == 0)
				{
					sort = 1;
				}
				else
				{
					// 最後の予約との重複チェック
					RequestSong lastRequestSong = requestSongContext.RequestSongs.OrderBy(x => x.Sort).Last();
					if (requestSong.Path == lastRequestSong.Path && requestSong.UserName == lastRequestSong.UserName)
					{
						// 重複している場合は既に登録されているので OK とする
						return Ok();
					}

					sort = requestSongContext.RequestSongs.Max(x => x.Sort) + 1;
				}

				// 追加する曲は最後
				requestSong.Sort = sort;
				requestSongContext.RequestSongs.Add(requestSong);
				requestSongContext.SaveChanges();
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約追加サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return BadRequest();
			}
		}

		// --------------------------------------------------------------------
		// 予約をすべて削除
		// --------------------------------------------------------------------
		[HttpPut, Route(YbdConstants.URL_DELETE_ALL)]
		public IActionResult DeleteAllSongs([FromBody] Int32 dummy)
		{
			try
			{
				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}
				requestSongContext.Database.EnsureDeleted();
				requestSongContext.Database.EnsureCreated();
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約全削除サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return BadRequest();
			}
		}

		// --------------------------------------------------------------------
		// 予約一覧を返す
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_LIST + "{query?}")]
		public IActionResult GetRequestSongs(String? query)
		{
			IEnumerable<RequestSong>? results = null;
			Int32 numResults = 0;
			DateTime lastModified = ServerConstants.INVALID_DATE;
			try
			{
				// キャッシュチェック
				lastModified = ServerCommon.LastModified(ServerConstants.FILE_NAME_REQUEST_SONGS);
				if (IsValidEntityTag(ServerCommon.DateTimeToModifiedJulianDate(lastModified)))
				{
					return NotModified();
				}

				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}

				Dictionary<String, String> parameters = YbdCommon.AnalyzeQuery(query);
				Int32 page = YbdCommon.GetPageFromQueryParameters(parameters);
				numResults = requestSongContext.RequestSongs.Count();
				results = requestSongContext.RequestSongs.OrderByDescending(x => x.Sort).Skip(YbdConstants.PAGE_SIZE * page).Take(YbdConstants.PAGE_SIZE).ToArray();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約一覧取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			if (results == null)
			{
				results = new RequestSong[0];
			}

			EntityTagHeaderValue eTag = GenerateEntityTag(ServerCommon.DateTimeToModifiedJulianDate(lastModified), YbdConstants.RESULT_PARAM_NAME_COUNT, numResults.ToString());
			return File(JsonSerializer.SerializeToUtf8Bytes(results), ServerConstants.MIME_TYPE_JSON, lastModified, eTag);
		}

		// --------------------------------------------------------------------
		// 予約者名一覧を返す（ゲストユーザーのみ）
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_GUEST_USER_NAMES)]
		public IActionResult GetUserNames()
		{
			IEnumerable<String>? results = null;
			Int32 numResults = 0;
			DateTime lastModified = ServerConstants.INVALID_DATE;
			try
			{
				// キャッシュチェック
				lastModified = ServerCommon.LastModified(ServerConstants.FILE_NAME_REQUEST_SONGS);
				if (IsValidEntityTag(ServerCommon.DateTimeToModifiedJulianDate(lastModified)))
				{
					return NotModified();
				}

				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}

				results = requestSongContext.RequestSongs.Where(x => x.UserId == 0).Select(x => x.UserName).GroupBy(y => y).Select(z => z.Key).ToArray();
				numResults = results.Count();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("予約者名一覧取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			if (results == null)
			{
				results = new String[0];
			}

			EntityTagHeaderValue eTag = GenerateEntityTag(ServerCommon.DateTimeToModifiedJulianDate(lastModified), YbdConstants.RESULT_PARAM_NAME_COUNT, numResults.ToString());
			return File(JsonSerializer.SerializeToUtf8Bytes(results), ServerConstants.MIME_TYPE_JSON, lastModified, eTag);
		}

		// ====================================================================
		// private メンバー関数
		// ====================================================================



	}
}
