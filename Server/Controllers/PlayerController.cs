// ============================================================================
// 
// 再生 API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 再生状態は頻繁に更新されるため ShortCache 属性を付与
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;

using System;
using System.Diagnostics;
using System.Linq;

using YukariBlazorDemo.Server.Database;
using YukariBlazorDemo.Server.Misc;
using YukariBlazorDemo.Shared.Database;
using YukariBlazorDemo.Shared.Misc;

namespace YukariBlazorDemo.Server.Controllers
{
	[ShortCache]
	[Produces(ServerConstants.MIME_TYPE_JSON)]
	[Route(YbdConstants.URL_API + YbdConstants.URL_PLAYER)]
	public class PlayerController : ApiController
	{
		// ====================================================================
		// API（ApiController）
		// ====================================================================

		// --------------------------------------------------------------------
		// 状態を返す
		// --------------------------------------------------------------------
		public override String ControllerStatus()
		{
			return "正常";
		}

		// ====================================================================
		// API（一般）
		// ====================================================================

		// --------------------------------------------------------------------
		// 再生中（または一時停止中）の曲を返す
		// --------------------------------------------------------------------
		[HttpGet, Route(YbdConstants.URL_PLAYING)]
		public RequestSong? GetPlayingSong()
		{
			try
			{
				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSongs == null)
				{
					throw new Exception();
				}
				RequestSong? result = requestSongContext.RequestSongs.Where(x => x.PlayStatus == PlayStatus.Playing || x.PlayStatus == PlayStatus.Pause).FirstOrDefault();
				if (result == null)
				{
					// 再生中の曲が無い
					result = new();
				}
				return result;
			}
			catch (Exception excep)
			{
				Debug.WriteLine("再生曲取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return null;
			}
		}

		// --------------------------------------------------------------------
		// 現在の曲を再生または一時停止する
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_PLAY_OR_PAUSE)]
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
			catch (Exception excep)
			{
				Debug.WriteLine("再生／一時停止サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
			}
			return BadRequest();
		}

		// --------------------------------------------------------------------
		// 次の曲を再生
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_NEXT)]
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
			catch (Exception excep)
			{
				Debug.WriteLine("次曲再生サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return BadRequest();
			}
		}

		// --------------------------------------------------------------------
		// 前の曲を再生
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_PREV)]
		public IActionResult Prev([FromBody] Int32 dummy)
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
					// 再生中・一時停止中の曲を未再生にする
					currentSong.PlayStatus = PlayStatus.Unplayed;
					requestSongContext.SaveChanges();
				}

				RequestSong? playedSong = requestSongContext.RequestSongs.Where(x => x.PlayStatus == PlayStatus.Played).OrderByDescending(x => x.Sort).FirstOrDefault();
				if (playedSong != null)
				{
					// 再生済みの曲を未再生にする
					playedSong.PlayStatus = PlayStatus.Unplayed;
					requestSongContext.SaveChanges();
				}
				else
				{
					if (currentSong == null)
					{
						// 再生済みの曲も再生中の曲も無い場合は戻れない
						throw new Exception();
					}
				}

				return PlayOrPause(dummy);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("前曲再生サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return BadRequest();
			}
		}
	}
}
