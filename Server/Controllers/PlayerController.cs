﻿// ============================================================================
// 
// 再生 API
// 
// ============================================================================

// ----------------------------------------------------------------------------
// 再生状態は頻繁に更新されるため ShortCache 属性を付与
// ----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System;
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
		public IActionResult GetPlayingSong()
		{
			try
			{
				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);
				RequestSong? result = requestSongs.FirstOrDefault(x => x.PlayStatus == PlayStatus.Playing || x.PlayStatus == PlayStatus.Pause);
				if (result == null)
				{
					return NotAcceptable();
				}
				return File(JsonSerializer.SerializeToUtf8Bytes(result), ServerConstants.MIME_TYPE_JSON);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("再生曲取得サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 現在の曲を再生または一時停止する
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_PLAY_OR_PAUSE)]
		public IActionResult PlayOrPause([FromBody] Int32 _)
		{
			try
			{
				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);
				RequestSong? requestSong;
				requestSong = requestSongs.FirstOrDefault(x => x.PlayStatus == PlayStatus.Playing);
				if (requestSong != null)
				{
					// 再生中の曲を一時停止する
					requestSong.PlayStatus = PlayStatus.Pause;
					requestSongContext.SaveChanges();
					return Ok();
				}

				requestSong = requestSongs.FirstOrDefault(x => x.PlayStatus == PlayStatus.Pause);
				if (requestSong != null)
				{
					// 一時停止中の曲を再生する
					requestSong.PlayStatus = PlayStatus.Playing;
					requestSongContext.SaveChanges();
					return Ok();
				}

				requestSong = requestSongs.Where(x => x.PlayStatus == PlayStatus.Unplayed).OrderBy(x => x.Sort).FirstOrDefault();
				if (requestSong != null)
				{
					// 未再生の曲を再生する
					requestSong.PlayStatus = PlayStatus.Playing;
					requestSongContext.SaveChanges();
					return Ok();
				}

				return NotAcceptable();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("再生／一時停止サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 次の曲を再生
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_NEXT)]
		public IActionResult Next([FromBody] Int32 dummy)
		{
			try
			{
				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				RequestSong? currentSong = requestSongs.FirstOrDefault(x => x.PlayStatus == PlayStatus.Playing || x.PlayStatus == PlayStatus.Pause);
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
				return InternalServerError();
			}
		}

		// --------------------------------------------------------------------
		// 指定曲に対する操作
		// --------------------------------------------------------------------
		[HttpPost, Route(YbdConstants.URL_REQUEST + "{requestSongId}")]
		public IActionResult Played(String requestSongId, [FromBody] String? command)
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

				switch (command)
				{
					case YbdConstants.REQUEST_PARAM_VALUE_PLAYED:
						requestSong.PlayStatus = PlayStatus.Played;
						break;
					case YbdConstants.REQUEST_PARAM_VALUE_UNPLAYED:
						requestSong.PlayStatus = PlayStatus.Unplayed;
						break;
					default:
						return BadRequest();
				}

				requestSongContext.SaveChanges();
				return Ok();
			}
			catch (Exception excep)
			{
				Debug.WriteLine("指定曲操作サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
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
				using RequestSongContext requestSongContext = CreateRequestSongContext(out DbSet<RequestSong> requestSongs);

				RequestSong? currentSong = requestSongs.FirstOrDefault(x => x.PlayStatus == PlayStatus.Playing || x.PlayStatus == PlayStatus.Pause);
				if (currentSong != null)
				{
					// 再生中・一時停止中の曲を未再生にする
					currentSong.PlayStatus = PlayStatus.Unplayed;
					requestSongContext.SaveChanges();
				}

				RequestSong? playedSong = requestSongs.Where(x => x.PlayStatus == PlayStatus.Played).OrderByDescending(x => x.Sort).FirstOrDefault();
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
						return NotAcceptable();
					}
				}

				return PlayOrPause(dummy);
			}
			catch (Exception excep)
			{
				Debug.WriteLine("前曲再生サーバーエラー：\n" + excep.Message);
				Debug.WriteLine("　スタックトレース：\n" + excep.StackTrace);
				return InternalServerError();
			}
		}
	}
}
