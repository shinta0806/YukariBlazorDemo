using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Shared;

namespace YukariBlazorDemo.Server.Controllers
{
	[Produces("application/json")]
	[Route("api/player")]
	public class PlayerController : Controller
	{
		public PlayerController()
		{

		}

		[HttpGet, Route("playing")]
		public RequestSong? GetPlayingSong()
		{
			RequestSong? result = null;
			try
			{
				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}
				result = requestSongContext.RequestSongs.Where(x => x.PlayStatus == PlayStatus.Playing || x.PlayStatus == PlayStatus.Pause).First();
			}
			catch (Exception)
			{
			}
			return result;
		}

		[HttpPost, Route("playorpause")]
		public IActionResult PlayOrPause([FromBody] Int32 dummy)
		{
			try
			{
				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}
				RequestSong? requestSong;

				requestSong = requestSongContext.RequestSongs.Where(x => x.PlayStatus == PlayStatus.Playing).FirstOrDefault();
				if (requestSong != null)
				{
					// 再生中の曲を一時停止する
					requestSong.PlayStatus = PlayStatus.Pause;
					requestSongContext.SaveChanges();
					return Ok();
				}

				requestSong = requestSongContext.RequestSongs.Where(x => x.PlayStatus == PlayStatus.Pause).FirstOrDefault();
				if (requestSong != null)
				{
					// 一時停止中の曲を再生する
					requestSong.PlayStatus = PlayStatus.Playing;
					requestSongContext.SaveChanges();
					return Ok();
				}

				requestSong = requestSongContext.RequestSongs.Where(x => x.PlayStatus == PlayStatus.Unplayed).OrderBy(x => x.Sort).FirstOrDefault();
				if (requestSong != null)
				{
					// 未再生の曲を再生する
					requestSong.PlayStatus = PlayStatus.Playing;
					requestSongContext.SaveChanges();
					return Ok();
				}
			}
			catch (Exception)
			{
			}

			return BadRequest();
		}

		[HttpPost, Route("next")]
		public IActionResult Next([FromBody] Int32 dummy)
		{
			try
			{
				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}

				RequestSong? currentSong = requestSongContext.RequestSongs.Where(x => x.PlayStatus == PlayStatus.Playing || x.PlayStatus == PlayStatus.Pause).FirstOrDefault();
				if (currentSong != null)
				{
					// 再生中・一時停止中の曲を再生済みにする
					currentSong.PlayStatus = PlayStatus.Played;
					requestSongContext.SaveChanges();
				}

				return PlayOrPause(dummy);
			}
			catch (Exception)
			{
			}
			return BadRequest();
		}



	}
}
