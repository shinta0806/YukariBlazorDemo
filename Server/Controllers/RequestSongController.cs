// ============================================================================
// 
// 予約 API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Controllers
{
	[Produces(ServerConstants.MIME_TYPE_JSON)]
	[Route(YbdConstants.URL_API + YbdConstants.URL_REQUEST_SONGS)]
	public class RequestSongController : Controller
	{
		// ====================================================================
		// API
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
					if (requestSong.Path == lastRequestSong.Path && requestSong.User == lastRequestSong.User)
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
				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}

				Dictionary<String, String> parameters = YbdCommon.AnalyzeQuery(query);
				Int32 page = YbdCommon.GetPageFromQueryParameters(parameters);
				numResults = requestSongContext.RequestSongs.Count();

				results = requestSongContext.RequestSongs.OrderByDescending(x => x.Sort).Skip(YbdConstants.PAGE_SIZE * page).Take(YbdConstants.PAGE_SIZE).ToArray();
				lastModified = ServerCommon.LastModified(ServerConstants.FILE_NAME_REQUEST_SONGS);
				//Debug.WriteLine("GetRequestSongs() lastModified: " + lastModified.ToString("yyyy/MM/dd HH:mm:ss"));
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

			EntityTagHeaderValue eTag = ServerCommon.GenerateEntityTag(ServerCommon.DateTimeToModifiedJulianDate(lastModified), YbdConstants.RESULT_PARAM_NAME_COUNT, numResults.ToString());
			return File(JsonSerializer.SerializeToUtf8Bytes(results), ServerConstants.MIME_TYPE_JSON, lastModified, eTag);
		}

		// --------------------------------------------------------------------
		// 状態を返す
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_STATUS)]
		public String RequestSongControllerStatus()
		{
			String status;
			try
			{
				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception("データベースにアクセスできません。");
				}

				// Where を使用すると列の不足を検出できる
				requestSongContext.RequestSongs.Where(x => x.RequestSongId == 0).FirstOrDefault();

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


	}
}
