using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Controllers
{
	[Produces("application/json")]
	[Route("api/requestsongs")]
	public class RequestSongController : Controller
	{
		public RequestSongController()
		{

		}

		[HttpGet]
		public IEnumerable<RequestSong> GetRequestSongs()
		{
			IEnumerable<RequestSong>? results = null;
			try
			{
				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}
				results = requestSongContext.RequestSongs.OrderByDescending(x => x.Sort).ToArray();
			}
			catch (Exception)
			{
			}
			if (results == null)
			{
				return new RequestSong[0];
			}
			return results;
		}

		[HttpPost, Route("request")]
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
				Debug.WriteLine(excep.Message);
				return BadRequest();
			}
		}

		[HttpPut, Route("deleteall")]
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
				requestSongContext.SaveChanges();
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine(excep.Message);
				return BadRequest();
			}
		}



	}
}
