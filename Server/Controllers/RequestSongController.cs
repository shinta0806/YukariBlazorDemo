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
				if (requestSongContext.RequestSong == null)
				{
					throw new Exception();
				}
				results = requestSongContext.RequestSong.ToArray();
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

		[HttpPost]
		public IActionResult AddRequestSong([FromBody] RequestSong requestSong)
		{
			try
			{
				if (!requestSong.IsValid())
				{
					throw new Exception();
				}

				using RequestSongContext requestSongContext = new();
				if (requestSongContext.RequestSong == null)
				{
					throw new Exception();
				}
				requestSongContext.RequestSong.Add(requestSong);
				requestSongContext.SaveChanges();
				return Ok();
			}
			catch (Exception)
			{
				return BadRequest();
			}
		}



	}
}
